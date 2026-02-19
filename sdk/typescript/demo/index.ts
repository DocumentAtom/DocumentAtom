import { DocumentAtomSdk } from 'documentatom-sdk';
import { ApiProcessorSettings, ChunkingConfiguration } from 'documentatom-sdk/dist/types/types';
import * as fs from 'fs';
import * as readline from 'readline';

const endpoint = process.argv[2] || 'http://localhost:8000/';
const api = new DocumentAtomSdk(endpoint);

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout,
});

function prompt(question: string, defaultValue?: string): Promise<string> {
  const display = defaultValue ? `${question} [${defaultValue}]: ` : `${question}: `;
  return new Promise((resolve) => {
    rl.question(display, (answer: string) => {
      resolve(answer.trim() || defaultValue || '');
    });
  });
}

async function promptYesNo(question: string, defaultValue: boolean = false): Promise<boolean> {
  const suffix = defaultValue ? '[Y/n]' : '[y/N]';
  const answer = await prompt(`${question} ${suffix}`);
  if (answer === '') return defaultValue;
  return answer.toLowerCase().startsWith('y');
}

function showMenu(): void {
  console.log();
  console.log('Available commands:');
  console.log('  ?               help, this menu');
  console.log('  q               quit');
  console.log('  cls             clear the screen');
  console.log(`  endpoint        show current endpoint (${endpoint})`);
  console.log();
  console.log('Health & Status:');
  console.log('  connectivity    check if server is reachable');
  console.log('  detect          test type detection');
  console.log();
  console.log('Document Processing:');
  console.log('  csv             process CSV document');
  console.log('  excel           process Excel document');
  console.log('  html            process HTML document');
  console.log('  json            process JSON document');
  console.log('  markdown        process Markdown document');
  console.log('  ocr             process image with OCR');
  console.log('  pdf             process PDF document');
  console.log('  png             process PNG image');
  console.log('  powerpoint      process PowerPoint document');
  console.log('  rtf             process RTF document');
  console.log('  text            process text document');
  console.log('  word            process Word document');
  console.log('  xml             process XML document');
  console.log();
}

async function testConnectivity(): Promise<void> {
  try {
    console.log('Checking server connectivity...');
    const result = await api.validateConnectivity();
    console.log(`Server is ${result ? 'reachable' : 'unreachable'}`);
  } catch (error: any) {
    console.log(`Error checking connectivity: ${error.message || error}`);
  }
}

async function testTypeDetection(): Promise<void> {
  try {
    const filename = await prompt('File path for type detection');
    if (!filename || !fs.existsSync(filename)) {
      console.log('File not found.');
      return;
    }

    const fileBinary = fs.readFileSync(filename);
    console.log(`Detecting type for ${filename} (${fileBinary.length} bytes)...`);

    const result = await api.TypeDetection.detectType(fileBinary);
    if (result) {
      console.log('Type detection result:');
      console.log(`  MIME Type: ${result.MimeType || 'Unknown'}`);
      console.log(`  Extension: ${result.Extension || 'Unknown'}`);
      console.log(`  Document Type: ${result.Type || 'Unknown'}`);
      console.log();
      console.log('Full JSON response:');
      console.log(JSON.stringify(result, null, 2));
    } else {
      console.log('No type detection result received.');
    }
  } catch (error: any) {
    console.log(`Error in type detection: ${error.message || error}`);
  }
}

async function configureSettings(documentType: string): Promise<ApiProcessorSettings | undefined> {
  const configure = await promptYesNo('Configure processor settings?', false);
  if (!configure) return undefined;

  const settings: ApiProcessorSettings = {};

  const ocrTypes = ['PDF', 'Word', 'Excel', 'PowerPoint', 'RTF'];
  if (ocrTypes.includes(documentType)) {
    const extractOcr = await promptYesNo('Extract atoms from images (OCR)?', false);
    if (extractOcr) settings.ExtractAtomsFromImages = true;
  }

  const enableChunking = await promptYesNo('Enable chunking?', false);
  if (enableChunking) {
    const strategy = await prompt(
      'Chunking strategy [FixedTokenCount/SentenceBased/ParagraphBased/RegexBased/WholeList/ListEntry/Row/RowWithHeaders/RowGroupWithHeaders/KeyValuePairs/WholeTable]',
      'FixedTokenCount'
    );

    const fixedTokenCountStr = await prompt('Fixed token count', '128');
    const fixedTokenCount = parseInt(fixedTokenCountStr, 10) || 128;

    const overlapStrategy = await prompt(
      'Overlap strategy [SlidingWindow/SentenceBoundaryAware/SemanticBoundaryAware]',
      'SlidingWindow'
    );

    const overlapCountStr = await prompt('Overlap count', '0');
    const overlapCount = parseInt(overlapCountStr, 10) || 0;

    settings.Chunking = {
      Enable: true,
      Strategy: strategy as ChunkingConfiguration['Strategy'],
      FixedTokenCount: fixedTokenCount,
      OverlapStrategy: overlapStrategy as ChunkingConfiguration['OverlapStrategy'],
      OverlapCount: overlapCount,
    };
  }

  return settings;
}

function displayAtoms(atoms: any[]): void {
  console.log(`Extracted ${atoms.length} atoms:`);
  console.log();

  const displayCount = Math.min(atoms.length, 5);
  for (let i = 0; i < displayCount; i++) {
    const atom = atoms[i];
    let content = '';
    if (atom.Type === 'Table') {
      content = `Table with ${atom.Rows || '?'} rows, ${atom.Columns || '?'} columns`;
    } else if (atom.Text) {
      content = atom.Text.substring(0, 300);
    } else if (atom.UnorderedList && atom.UnorderedList.length > 0) {
      content = `Unordered list with ${atom.UnorderedList.length} items`;
    } else if (atom.OrderedList && atom.OrderedList.length > 0) {
      content = `Ordered list with ${atom.OrderedList.length} items`;
    } else {
      content = 'No text content';
    }

    console.log(`  Atom [${atom.Position}]: ${atom.Type} - ${content}`);

    if (atom.Chunks && atom.Chunks.length > 0) {
      console.log(`    Chunks: ${atom.Chunks.length}`);
      const chunkDisplayCount = Math.min(atom.Chunks.length, 3);
      for (let j = 0; j < chunkDisplayCount; j++) {
        const chunk = atom.Chunks[j];
        const chunkPreview = chunk.Text
          ? chunk.Text.substring(0, 100)
          : 'No text';
        console.log(`      Chunk [${chunk.Position}]: ${chunk.Length} chars - ${chunkPreview}...`);
      }
      if (atom.Chunks.length > 3) {
        console.log(`      ... and ${atom.Chunks.length - 3} more chunks`);
      }
    }
  }

  if (atoms.length > 5) {
    console.log(`... and ${atoms.length - 5} more atoms`);
  }

  // Chunking summary
  const atomsWithChunks = atoms.filter((a) => a.Chunks && a.Chunks.length > 0).length;
  const totalChunks = atoms.reduce((sum, a) => sum + (a.Chunks ? a.Chunks.length : 0), 0);
  if (atomsWithChunks > 0) {
    console.log();
    console.log(`Chunking summary: ${atomsWithChunks} atoms with chunks, ${totalChunks} total chunks`);
  }

  console.log();
  console.log('Full result:');
  console.log(JSON.stringify(atoms, null, 2));
}

type ExtractMethod = (fileBinary: Buffer, settings?: ApiProcessorSettings) => Promise<any>;

async function testDocumentProcessing(documentType: string, extractMethod: ExtractMethod): Promise<void> {
  try {
    const filename = await prompt(`File path for ${documentType} processing`);
    if (!filename || !fs.existsSync(filename)) {
      console.log('File not found.');
      return;
    }

    const settings = await configureSettings(documentType);

    const fileBinary = fs.readFileSync(filename);
    console.log(`Processing ${documentType} file: ${filename} (${fileBinary.length} bytes)...`);

    const startTime = Date.now();
    const atoms = await extractMethod(fileBinary, settings);
    const elapsed = Date.now() - startTime;

    if (atoms && Array.isArray(atoms) && atoms.length > 0) {
      console.log(`Processing completed in ${elapsed.toFixed(2)}ms`);
      displayAtoms(atoms);
    } else {
      console.log('No atoms extracted.');
    }
  } catch (error: any) {
    console.log(`Error processing ${documentType}: ${error.message || error}`);
  }
}

const commands: Record<string, () => Promise<void>> = {
  connectivity: testConnectivity,
  detect: testTypeDetection,
  csv: () => testDocumentProcessing('CSV', (f, s) => api.ExtractAtom.csv(f, s)),
  excel: () => testDocumentProcessing('Excel', (f, s) => api.ExtractAtom.excel(f, s)),
  html: () => testDocumentProcessing('HTML', (f, s) => api.ExtractAtom.html(f, s)),
  json: () => testDocumentProcessing('JSON', (f, s) => api.ExtractAtom.json(f, s)),
  markdown: () => testDocumentProcessing('Markdown', (f, s) => api.ExtractAtom.markdown(f, s)),
  ocr: () => testDocumentProcessing('OCR', (f, s) => api.ExtractAtom.ocr(f, s)),
  pdf: () => testDocumentProcessing('PDF', (f, s) => api.ExtractAtom.pdf(f, s)),
  png: () => testDocumentProcessing('PNG', (f, s) => api.ExtractAtom.png(f, s)),
  powerpoint: () => testDocumentProcessing('PowerPoint', (f, s) => api.ExtractAtom.powerpoint(f, s)),
  rtf: () => testDocumentProcessing('RTF', (f, s) => api.ExtractAtom.rtf(f, s)),
  text: () => testDocumentProcessing('Text', (f, s) => api.ExtractAtom.text(f, s)),
  word: () => testDocumentProcessing('Word', (f, s) => api.ExtractAtom.word(f, s)),
  xml: () => testDocumentProcessing('XML', (f, s) => api.ExtractAtom.xml(f, s)),
};

async function main(): Promise<void> {
  console.log('DocumentAtom SDK Test Application (TypeScript)');
  console.log('==============================================');
  console.log(`SDK initialized with endpoint: ${endpoint}`);
  console.log();

  while (true) {
    const input = await prompt('Command [? for help]');

    if (input === '?') {
      showMenu();
    } else if (input === 'q') {
      break;
    } else if (input === 'cls') {
      console.clear();
    } else if (input === 'endpoint') {
      console.log(`Current endpoint: ${endpoint}`);
    } else if (commands[input]) {
      await commands[input]();
    } else {
      console.log("Unknown command. Type '?' for help.");
    }
  }

  rl.close();
  console.log('Goodbye!');
}

main();

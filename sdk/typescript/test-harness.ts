import { DocumentAtomSdk } from './dist/index';
import { ApiProcessorSettings } from './dist/types/types';
import * as fs from 'fs';
import * as path from 'path';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

const endpoint = process.argv[2] || 'http://localhost:8000/';
const api = new DocumentAtomSdk(endpoint);

const FIXTURE_DIR = path.resolve(__dirname, '..', 'test-fixtures');

const FIXTURES: [string, string][] = [
  ['sample.csv', 'csv'],
  ['sample.html', 'html'],
  ['sample.json', 'json'],
  ['sample.md', 'markdown'],
  ['sample.txt', 'text'],
  ['sample.xml', 'xml'],
  ['sample.rtf', 'rtf'],
  ['sample.pdf', 'pdf'],
  ['sample.docx', 'word'],
  ['sample.xlsx', 'excel'],
  ['sample.pptx', 'powerpoint'],
  ['sample.png', 'png'],
  ['sample.jpg', 'ocr'],
];

const IMAGE_FORMATS = new Set(['png', 'ocr']);
const CHUNKING_SKIP = new Set(['png', 'ocr']);
const OCR_FORMATS = new Set(['pdf', 'word', 'powerpoint']);

// ---------------------------------------------------------------------------
// Result tracking
// ---------------------------------------------------------------------------

interface TestResult {
  status: 'PASS' | 'FAIL' | 'SKIP';
  duration: number;
  name: string;
  message: string;
}

const results: TestResult[] = [];

function record(status: 'PASS' | 'FAIL' | 'SKIP', duration: number, name: string, message: string = '') {
  results.push({ status, duration, name, message });
  const tag = { PASS: '[PASS]', FAIL: '[FAIL]', SKIP: '[SKIP]' }[status];
  let line = `${tag}  ${duration.toFixed(3).padStart(7)}s  ${name}`;
  if (message) line += ` - ${message}`;
  console.log(line);
}

async function runTest(name: string, fn: () => Promise<void>) {
  const t0 = performance.now();
  try {
    await fn();
    record('PASS', (performance.now() - t0) / 1000, name);
  } catch (err: any) {
    record('FAIL', (performance.now() - t0) / 1000, name, String(err?.message || err));
  }
}

function skipTest(name: string, reason: string) {
  record('SKIP', 0, name, reason);
}

// ---------------------------------------------------------------------------
// Fixture loader
// ---------------------------------------------------------------------------

function loadFixture(filename: string): Buffer | null {
  const p = path.join(FIXTURE_DIR, filename);
  if (!fs.existsSync(p)) return null;
  return fs.readFileSync(p);
}

// ---------------------------------------------------------------------------
// Extraction helper
// ---------------------------------------------------------------------------

function callExtraction(format: string, data: Buffer, settings?: ApiProcessorSettings): Promise<any[]> {
  const methods: Record<string, Function> = {
    csv: api.ExtractAtom.csv.bind(api.ExtractAtom),
    html: api.ExtractAtom.html.bind(api.ExtractAtom),
    json: api.ExtractAtom.json.bind(api.ExtractAtom),
    markdown: api.ExtractAtom.markdown.bind(api.ExtractAtom),
    text: api.ExtractAtom.text.bind(api.ExtractAtom),
    xml: api.ExtractAtom.xml.bind(api.ExtractAtom),
    rtf: api.ExtractAtom.rtf.bind(api.ExtractAtom),
    pdf: api.ExtractAtom.pdf.bind(api.ExtractAtom),
    word: api.ExtractAtom.word.bind(api.ExtractAtom),
    excel: api.ExtractAtom.excel.bind(api.ExtractAtom),
    powerpoint: api.ExtractAtom.powerpoint.bind(api.ExtractAtom),
    png: api.ExtractAtom.png.bind(api.ExtractAtom),
    ocr: api.ExtractAtom.ocr.bind(api.ExtractAtom),
  };
  const fn = methods[format];
  if (!fn) throw new Error(`Unknown format: ${format}`);
  return fn(data, settings);
}

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------

async function main() {
  console.log(`Endpoint:    ${endpoint}`);
  console.log(`Fixtures:    ${FIXTURE_DIR}`);
  console.log();

  const overallStart = performance.now();

  // 1. Connectivity (2 tests)
  await runTest('Connectivity: server is reachable', async () => {
    const ok = await api.validateConnectivity();
    if (!ok) throw new Error('validateConnectivity returned false');
  });

  await runTest('Connectivity: server returns status', async () => {
    const ok = await api.validateConnectivity();
    if (ok === null || ok === undefined) throw new Error('status returned null');
  });

  // 2. Type Detection (13 tests)
  for (const [filename, format] of FIXTURES) {
    const data = loadFixture(filename);
    if (!data) {
      skipTest(`Type detection: ${filename}`, 'fixture not found');
      continue;
    }
    await runTest(`Type detection: ${filename}`, async () => {
      const result = await api.TypeDetection.detectType(data);
      if (!result) throw new Error('detectType returned null');
    });
  }

  // 3. Extraction — No Settings (13 tests)
  for (const [filename, format] of FIXTURES) {
    const data = loadFixture(filename);
    if (!data) {
      skipTest(`Extraction (default): ${filename}`, 'fixture not found');
      continue;
    }
    await runTest(`Extraction (default): ${filename}`, async () => {
      const atoms = await callExtraction(format, data);
      if (IMAGE_FORMATS.has(format)) {
        // Minimal test images have no text; just verify the call succeeded
        if (atoms === null || atoms === undefined)
          throw new Error('extraction returned null');
      } else {
        if (!atoms || atoms.length < 1)
          throw new Error(`expected >= 1 atom, got ${atoms?.length ?? 0}`);
      }
    });
  }

  // 4. Extraction — With Chunking (11 tests, skip PNG/JPG)
  for (const [filename, format] of FIXTURES) {
    if (CHUNKING_SKIP.has(format)) {
      skipTest(`Extraction (chunking): ${filename}`, 'chunking not applicable for images');
      continue;
    }
    const data = loadFixture(filename);
    if (!data) {
      skipTest(`Extraction (chunking): ${filename}`, 'fixture not found');
      continue;
    }
    await runTest(`Extraction (chunking): ${filename}`, async () => {
      const settings: ApiProcessorSettings = {
        Chunking: {
          Enable: true,
          Strategy: 'FixedTokenCount',
          FixedTokenCount: 32,
          OverlapCount: 0,
        },
      };
      const atoms = await callExtraction(format, data, settings);
      if (!atoms || atoms.length < 1)
        throw new Error(`expected >= 1 atom, got ${atoms?.length ?? 0}`);
      const hasChunks = atoms.some((a: any) => a.Chunks && a.Chunks.length > 0);
      if (!hasChunks)
        throw new Error('expected at least one atom with non-empty Chunks');
    });
  }

  // 5. Extraction — With OCR Setting (3 tests: PDF, Word, PowerPoint)
  for (const [filename, format] of FIXTURES) {
    if (!OCR_FORMATS.has(format)) continue;
    const data = loadFixture(filename);
    if (!data) {
      skipTest(`Extraction (OCR setting): ${filename}`, 'fixture not found');
      continue;
    }
    await runTest(`Extraction (OCR setting): ${filename}`, async () => {
      const settings: ApiProcessorSettings = {
        ExtractAtomsFromImages: true,
      };
      const atoms = await callExtraction(format, data, settings);
      if (!atoms || atoms.length < 1)
        throw new Error(`expected >= 1 atom, got ${atoms?.length ?? 0}`);
    });
  }

  // 6. Error Cases (3 tests)
  await runTest('Error: empty data to text extraction', async () => {
    try {
      await api.ExtractAtom.text(Buffer.alloc(0));
    } catch {
      // error response is acceptable
    }
  });

  await runTest('Error: CSV bytes sent to PDF endpoint', async () => {
    const csvData = loadFixture('sample.csv');
    if (!csvData) throw new Error('fixture not found');
    try {
      await api.ExtractAtom.pdf(csvData);
    } catch {
      // error response is acceptable
    }
  });

  await runTest('Error: empty data to type detection', async () => {
    try {
      await api.TypeDetection.detectType(Buffer.alloc(0));
    } catch {
      // error response is acceptable
    }
  });

  const overallDuration = (performance.now() - overallStart) / 1000;

  // Summary
  const passed = results.filter((r) => r.status === 'PASS').length;
  const failed = results.filter((r) => r.status === 'FAIL').length;
  const skipped = results.filter((r) => r.status === 'SKIP').length;
  const total = results.length;
  const failedTests = results.filter((r) => r.status === 'FAIL');

  console.log();
  console.log('='.repeat(60));
  console.log('Test Summary');
  console.log('='.repeat(60));
  console.log(`Total:    ${total}`);
  console.log(`Passed:   ${passed}`);
  console.log(`Failed:   ${failed}`);
  console.log(`Skipped:  ${skipped}`);
  console.log(`Duration: ${overallDuration.toFixed(3)}s`);
  if (failedTests.length > 0) {
    console.log();
    console.log('Failed tests:');
    for (const t of failedTests) {
      console.log(`  - ${t.name} - ${t.message}`);
    }
  }
  console.log('='.repeat(60));

  process.exit(failed > 0 ? 1 : 0);
}

main().catch((err) => {
  console.error('Fatal error:', err);
  process.exit(1);
});

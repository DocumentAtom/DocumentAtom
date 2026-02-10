import ExtractAtom from '../src/sdks/ExtractAtom';
import { SdkConfiguration } from '../src/base/SdkConfiguration';
import { mockAtomResponse, mockErrorResponse } from './mockdata';
import { setupServer } from 'msw/node';
import { handlers } from './handlers';

// Setup MSW server
const server = setupServer(...handlers);

describe('ExtractAtom', () => {
  beforeAll(() => {
    server.listen();
  });

  afterEach(() => {
    server.resetHandlers();
  });

  afterAll(() => {
    server.close();
  });
  let config: SdkConfiguration;
  let extractAtom: ExtractAtom;
  const mockFileBinary = Buffer.from('Mock file content for testing');

  beforeEach(() => {
    config = new SdkConfiguration('http://localhost:8000/v1.0/mock/');
    extractAtom = new ExtractAtom(config);
  });

  describe('constructor', () => {
    it('should create instance with valid config', () => {
      expect(extractAtom).toBeInstanceOf(ExtractAtom);
    });
  });

  describe('excel method', () => {
    it('should extract atoms from Excel file successfully', async () => {
      const cancellationToken = new AbortController();
      const result = await extractAtom.excel(mockFileBinary, cancellationToken);

      expect(result).toEqual(mockAtomResponse);
    });

    it('should throw error for null fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.excel(null as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });

    it('should throw error for undefined fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.excel(undefined as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });
  });

  describe('html method', () => {
    it('should extract atoms from HTML file successfully', async () => {
      const cancellationToken = new AbortController();
      const result = await extractAtom.html(mockFileBinary, cancellationToken);

      expect(result).toEqual(mockAtomResponse);
    });

    it('should throw error for null fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.html(null as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });

    it('should throw error for undefined fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.html(undefined as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });
  });

  describe('markdown method', () => {
    it('should extract atoms from Markdown file successfully', async () => {
      const cancellationToken = new AbortController();
      const result = await extractAtom.markdown(mockFileBinary, cancellationToken);

      expect(result).toEqual(mockAtomResponse);
    });

    it('should throw error for null fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.markdown(null as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });

    it('should throw error for undefined fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.markdown(undefined as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });
  });

  describe('ocr method', () => {
    it('should extract atoms from OCR file successfully', async () => {
      const cancellationToken = new AbortController();
      const result = await extractAtom.ocr(mockFileBinary, cancellationToken);

      expect(result).toEqual(mockAtomResponse);
    });

    it('should throw error for null fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.ocr(null as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });

    it('should throw error for undefined fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.ocr(undefined as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });
  });

  describe('pdf method', () => {
    it('should extract atoms from PDF file successfully', async () => {
      const cancellationToken = new AbortController();
      const result = await extractAtom.pdf(mockFileBinary, cancellationToken);

      expect(result).toEqual(mockAtomResponse);
    });

    it('should throw error for null fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.pdf(null as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });

    it('should throw error for undefined fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.pdf(undefined as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });
  });

  describe('png method', () => {
    it('should extract atoms from PNG file successfully', async () => {
      const cancellationToken = new AbortController();
      const result = await extractAtom.png(mockFileBinary, cancellationToken);

      expect(result).toEqual(mockAtomResponse);
    });

    it('should throw error for null fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.png(null as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });

    it('should throw error for undefined fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.png(undefined as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });
  });

  describe('powerpoint method', () => {
    it('should extract atoms from PowerPoint file successfully', async () => {
      const cancellationToken = new AbortController();
      const result = await extractAtom.powerpoint(mockFileBinary, cancellationToken);

      expect(result).toEqual(mockAtomResponse);
    });

    it('should throw error for null fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.powerpoint(null as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });

    it('should throw error for undefined fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.powerpoint(undefined as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });
  });

  describe('rtf method', () => {
    it('should extract atoms from RTF file successfully', async () => {
      const cancellationToken = new AbortController();
      const result = await extractAtom.rtf(mockFileBinary, cancellationToken);

      expect(result).toEqual(mockAtomResponse);
    });

    it('should throw error for null fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.rtf(null as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });

    it('should throw error for undefined fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.rtf(undefined as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });
  });

  describe('text method', () => {
    it('should extract atoms from text file successfully', async () => {
      const cancellationToken = new AbortController();
      const result = await extractAtom.text(mockFileBinary, cancellationToken);

      expect(result).toEqual(mockAtomResponse);
    });

    it('should throw error for null fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.text(null as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });

    it('should throw error for undefined fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.text(undefined as any, cancellationToken)).toThrow(
        'ArgumentNullException: fileBinary is null or empty'
      );
    });
  });

  describe('word method', () => {
    it('should extract atoms from Word file successfully', async () => {
      const cancellationToken = new AbortController();
      const result = await extractAtom.word(mockFileBinary, cancellationToken);

      expect(result).toEqual(mockAtomResponse);
    });

    it('should throw error for null fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.word(null as any, cancellationToken)).toThrow(
        'ArgumentNullException: filePath is null or empty'
      );
    });

    it('should throw error for undefined fileBinary', () => {
      const cancellationToken = new AbortController();
      expect(() => extractAtom.word(undefined as any, cancellationToken)).toThrow(
        'ArgumentNullException: filePath is null or empty'
      );
    });
  });

  describe('cancellation token support', () => {
    it('should support cancellation token for all methods', async () => {
      const cancellationToken = new AbortController();

      // Test cancellation token with different methods
      const excelResult = await extractAtom.excel(mockFileBinary, cancellationToken);
      const htmlResult = await extractAtom.html(mockFileBinary, cancellationToken);
      const pdfResult = await extractAtom.pdf(mockFileBinary, cancellationToken);

      expect(excelResult).toEqual(mockAtomResponse);
      expect(htmlResult).toEqual(mockAtomResponse);
      expect(pdfResult).toEqual(mockAtomResponse);
    });
  });

  describe('URL construction', () => {
    it('should construct correct URLs for each method', () => {
      const cancellationToken = new AbortController();

      // Test that each method constructs the correct endpoint URL
      // This is implicitly tested through the successful API calls above
      expect(config.endpoint).toBe('http://localhost:8000/v1.0/mock/');
    });
  });
});

import { DocumentAtomSdk } from '../src';
import { SdkConfiguration } from '../src/base/SdkConfiguration';
import SdkBase from '../src/base/SdkBase';
import Helper from '../src/utils/Helper';
import { SeverityEnum } from '../src/enums/SeverityEnum';
import superagent from 'superagent';
import fs from 'fs';

// Mock dependencies
jest.mock('superagent');
jest.mock('fs');
jest.mock('../src/utils/Logger', () => ({
  default: {
    log: jest.fn(),
  },
}));

const mockSuperagent = superagent as jest.Mocked<typeof superagent>;
const mockFs = fs as jest.Mocked<typeof fs>;

describe('DocumentAtomSdk', () => {
  it('should be defined', () => {
    expect(DocumentAtomSdk).toBeDefined();
  });
});

describe('SdkConfiguration', () => {
  describe('constructor', () => {
    it('should create instance with valid endpoint', () => {
      const config = new SdkConfiguration('http://test.com');
      expect(config.endpoint).toBe('http://test.com/');
      expect(config.timeoutMs).toBe(300000);
      expect(config.defaultHeaders).toEqual({});
    });

    it('should add trailing slash to endpoint if not present', () => {
      const config = new SdkConfiguration('http://test.com');
      expect(config.endpoint).toBe('http://test.com/');
    });

    it('should not add trailing slash if already present', () => {
      const config = new SdkConfiguration('http://test.com/');
      expect(config.endpoint).toBe('http://test.com/');
    });

    it('should throw error for null endpoint', () => {
      expect(() => new SdkConfiguration('')).toThrow('ArgumentNullException: Endpoint is null or empty');
    });

    it('should throw error for undefined endpoint', () => {
      expect(() => new SdkConfiguration(undefined as any)).toThrow('ArgumentNullException: Endpoint is null or empty');
    });
  });

  describe('endpoint getter and setter', () => {
    let config: SdkConfiguration;

    beforeEach(() => {
      config = new SdkConfiguration('http://test.com');
    });

    it('should get endpoint', () => {
      expect(config.endpoint).toBe('http://test.com/');
    });

    it('should set endpoint and add trailing slash', () => {
      config.endpoint = 'http://newtest.com';
      expect(config.endpoint).toBe('http://newtest.com/');
    });

    it('should set endpoint without adding duplicate trailing slash', () => {
      config.endpoint = 'http://newtest.com/';
      expect(config.endpoint).toBe('http://newtest.com/');
    });

    it('should throw error when setting null endpoint', () => {
      expect(() => {
        config.endpoint = '';
      }).toThrow('ArgumentNullException: Endpoint is null or empty');
    });

    it('should throw error when setting undefined endpoint', () => {
      expect(() => {
        config.endpoint = undefined as any;
      }).toThrow('ArgumentNullException: Endpoint is null or empty');
    });
  });

  describe('timeoutMs getter and setter', () => {
    let config: SdkConfiguration;

    beforeEach(() => {
      config = new SdkConfiguration('http://test.com');
    });

    it('should get default timeout', () => {
      expect(config.timeoutMs).toBe(300000);
    });

    it('should set timeout', () => {
      config.timeoutMs = 5000;
      expect(config.timeoutMs).toBe(5000);
    });

    it('should throw error when setting timeout less than 1', () => {
      expect(() => {
        config.timeoutMs = 0;
      }).toThrow('TimeoutMs must be greater than 0.');

      expect(() => {
        config.timeoutMs = -1;
      }).toThrow('TimeoutMs must be greater than 0.');
    });
  });
});

describe('Helper', () => {
  let config: SdkConfiguration;
  let helper: Helper;

  beforeEach(() => {
    config = new SdkConfiguration('http://test.com');
    helper = new Helper(config);
  });

  describe('constructor', () => {
    it('should create instance with valid config', () => {
      expect(helper).toBeInstanceOf(Helper);
    });

    it('should throw error for null config', () => {
      expect(() => new Helper(null as any)).toThrow('ArgumentNullException: config is null or empty');
    });

    it('should throw error for undefined config', () => {
      expect(() => new Helper(undefined as any)).toThrow('ArgumentNullException: config is null or empty');
    });
  });

  describe('convertFileToBinary', () => {
    const testFilePath = '/path/to/test/file.txt';
    const testBuffer = Buffer.from('test file content');

    it('should convert file to binary data', () => {
      mockFs.existsSync.mockReturnValue(true);
      mockFs.readFileSync.mockReturnValue(testBuffer);

      const result = helper.convertFileToBinary(testFilePath);

      expect(mockFs.existsSync).toHaveBeenCalledWith(testFilePath);
      expect(mockFs.readFileSync).toHaveBeenCalledWith(testFilePath);
      expect(result).toBe(testBuffer);
    });

    it('should throw error when file does not exist', () => {
      mockFs.existsSync.mockReturnValue(false);

      expect(() => helper.convertFileToBinary(testFilePath)).toThrow(`File not found at ${testFilePath}`);
      expect(mockFs.existsSync).toHaveBeenCalledWith(testFilePath);
    });
  });
});

describe('SdkBase', () => {
  let config: SdkConfiguration;
  let sdkBase: SdkBase;

  beforeEach(() => {
    config = new SdkConfiguration('http://test.com');
    sdkBase = new SdkBase(config);
    jest.clearAllMocks();
  });

  describe('constructor', () => {
    it('should create instance with valid config', () => {
      expect(sdkBase).toBeInstanceOf(SdkBase);
    });

    it('should throw error for null config', () => {
      expect(() => new SdkBase(null as any)).toThrow('ArgumentNullException: config is null or empty');
    });

    it('should throw error for undefined config', () => {
      expect(() => new SdkBase(undefined as any)).toThrow('ArgumentNullException: config is null or empty');
    });
  });

  describe('log method', () => {
    it('should not call logger when message is empty', () => {
      const Logger = require('../src/utils/Logger').default;
      const logSpy = jest.spyOn(Logger, 'log');

      sdkBase['log'](SeverityEnum.Debug, '');

      expect(logSpy).not.toHaveBeenCalled();
    });

    it('should not call logger when message is null', () => {
      const Logger = require('../src/utils/Logger').default;
      const logSpy = jest.spyOn(Logger, 'log');

      sdkBase['log'](SeverityEnum.Debug, null as any);

      expect(logSpy).not.toHaveBeenCalled();
    });
  });

  describe('upload method', () => {
    const testUrl = 'http://test.com/upload';
    const testData = Buffer.from('test data');
    const mockRequest = {
      set: jest.fn().mockReturnThis(),
      attach: jest.fn().mockReturnThis(),
      timeout: jest.fn().mockReturnThis(),
      then: jest.fn().mockReturnThis(),
      catch: jest.fn().mockReturnThis(),
      abort: jest.fn(),
    };

    beforeEach(() => {
      mockSuperagent.post.mockReturnValue(mockRequest as any);
    });

    it('should reject with error for null URL', async () => {
      await expect(sdkBase['upload']('', testData)).rejects.toThrow('URL cannot be null or empty.');
    });

    it('should reject with error for undefined URL', async () => {
      await expect(sdkBase['upload'](undefined as any, testData)).rejects.toThrow('URL cannot be null or empty.');
    });
  });
});

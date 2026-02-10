import { ExtractAtomResponse } from '../src/types';

export const mockAtomResponse: ExtractAtomResponse = [
  {
    GUID: '1',
    Type: 'Excel',
    Position: 1,
    Length: 100,
    MD5Hash: '1234567890',
    SHA1Hash: '1234567890',
    SHA256Hash: '1234567890',
    Formatting: '1234567890',
    Text: '1234567890',
  },
];

export const mockErrorResponse = {
  error: 'File processing failed',
  message: 'Invalid file format',
  code: 400,
};

import { http, HttpResponse } from 'msw';
import { mockEndpoint } from './setupTest';
import { mockAtomResponse, mockErrorResponse } from './mockdata';

export const handlers = [
  // Check if a edge exists by GUID
  http.head(mockEndpoint, ({ request, params, cookies }) => {
    return HttpResponse.text('Hello'); // Simulating edge exists
  }),

  // Test successful GET request
  http.get(`${mockEndpoint}/test-get`, () => {
    return HttpResponse.json({ data: 'test' });
  }),

  // ExtractAtom endpoints
  http.post(`${mockEndpoint}atom/excel`, () => {
    return HttpResponse.json(mockAtomResponse);
  }),

  http.post(`${mockEndpoint}atom/html`, () => {
    return HttpResponse.json(mockAtomResponse);
  }),

  http.post(`${mockEndpoint}atom/markdown`, () => {
    return HttpResponse.json(mockAtomResponse);
  }),

  http.post(`${mockEndpoint}atom/ocr`, () => {
    return HttpResponse.json(mockAtomResponse);
  }),

  http.post(`${mockEndpoint}atom/pdf`, () => {
    return HttpResponse.json(mockAtomResponse);
  }),

  http.post(`${mockEndpoint}atom/png`, () => {
    return HttpResponse.json(mockAtomResponse);
  }),

  http.post(`${mockEndpoint}atom/powerpoint`, () => {
    return HttpResponse.json(mockAtomResponse);
  }),

  http.post(`${mockEndpoint}atom/rtf`, () => {
    return HttpResponse.json(mockAtomResponse);
  }),

  http.post(`${mockEndpoint}atom/text`, () => {
    return HttpResponse.json(mockAtomResponse);
  }),

  http.post(`${mockEndpoint}atom/word`, () => {
    return HttpResponse.json(mockAtomResponse);
  }),

  // Error endpoints
  http.post(`${mockEndpoint}atom/error`, () => {
    return HttpResponse.json(mockErrorResponse, { status: 400 });
  }),
];

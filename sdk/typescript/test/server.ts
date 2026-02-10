import { setupServer } from 'msw/node';

export const getServer = (handlers: any) => {
  return setupServer(...handlers);
};

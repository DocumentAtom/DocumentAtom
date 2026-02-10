export default class GenericExceptionHandlers {
  /**
   * @param {string} argName
   */
  static ArgumentNullException = (argName: string) => {
    throw Error(`ArgumentNullException: ${argName} is null or empty`);
  };
  static GenericException = (message: string) => {
    throw Error(message);
  };
}

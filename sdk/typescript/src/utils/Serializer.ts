export default class Serializer {
  /**
   * Deserialize JSON to an instance of the specified type.
   * @param {jsonString} jsonString
   * @return {Object}
   */
  static deserializeJson(response: string): any {
    if (typeof response === 'string') {
      try {
        const data = JSON.parse(response);
        return data;
      } catch (err) {
        // eslint-disable-next-line no-console
        console.log(err);
        return response;
      }
    }
    return response;
  }

  /**
   * Serialize an object to JSON.
   * @param {object} obj - Object to serialize.
   * @param {boolean} pretty - Whether to pretty print the JSON.
   * @returns {string} - Serialized JSON string.
   */
  static serializeJson(obj: any, pretty = true) {
    if (obj === null) return null;

    return JSON.stringify(obj, this.jsonReplacer, pretty ? 2 : 0);
  }

  // JSON Replacer to handle specific types
  static jsonReplacer(key: string, value: any) {
    if (value instanceof Date) {
      return value.toISOString();
    }
    return value;
  }
}

export default class Serializer {
    /**
     * Deserialize JSON to an instance of the specified type.
     * @param {jsonString} jsonString
     * @return {Object}
     */
    static deserializeJson(response: string): any;
    /**
     * Serialize an object to JSON.
     * @param {object} obj - Object to serialize.
     * @param {boolean} pretty - Whether to pretty print the JSON.
     * @returns {string} - Serialized JSON string.
     */
    static serializeJson(obj: any, pretty?: boolean): string | null;
    static jsonReplacer(key: string, value: any): any;
}

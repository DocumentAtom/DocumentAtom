import { SeverityEnum } from '../enums/SeverityEnum';
export default class Logger {
    /**
     * @param {string} type
     * @param {string} message
     */
    static log: (severity: SeverityEnum, message: string) => void;
}
export declare const LoggerInstance: Logger;

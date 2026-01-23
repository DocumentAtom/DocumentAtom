import { formatDateTime, formatSecondsForTimer } from "#/utils/dateUtils";
import moment from "moment";

describe("dateUtils", () => {
  describe("formatDateTime", () => {
    it("should format date with default format", () => {
      const dateTime = "2024-01-15T14:30:00Z";
      const result = formatDateTime(dateTime);

      // Default format: 'Do MMM YYYY, HH:mm'
      expect(result).toMatch(
        /\d{1,2}(st|nd|rd|th) [A-Z][a-z]{2} \d{4}, \d{2}:\d{2}/
      );
    });

    it("should format date with custom format", () => {
      const dateTime = "2024-01-15T14:30:00Z";
      const customFormat = "YYYY-MM-DD";
      const result = formatDateTime(dateTime, customFormat);

      expect(result).toMatch(/\d{4}-\d{2}-\d{2}/);
    });

    it("should format date with time format", () => {
      const dateTime = "2024-01-15T14:30:00Z";
      const timeFormat = "HH:mm:ss";
      const result = formatDateTime(dateTime, timeFormat);

      expect(result).toMatch(/\d{2}:\d{2}:\d{2}/);
    });

    it("should handle different date formats", () => {
      const dates = [
        "2024-01-15",
        "2024/01/15",
        "2024-01-15T14:30:00",
        "2024-01-15T14:30:00.000Z",
      ];

      dates.forEach((date) => {
        const result = formatDateTime(date);
        expect(result).not.toBe("Invalid Date");
      });
    });

    it("should return 'Invalid Date' for empty string", () => {
      const result = formatDateTime("");
      expect(result).toBe("Invalid Date");
    });

    it("should return 'Invalid Date' for null", () => {
      const result = formatDateTime(null as any);
      expect(result).toBe("Invalid Date");
    });

    it("should return 'Invalid Date' for undefined", () => {
      const result = formatDateTime(undefined as any);
      expect(result).toBe("Invalid Date");
    });

    it("should handle ISO 8601 format", () => {
      const dateTime = "2024-01-15T14:30:00.000Z";
      const result = formatDateTime(dateTime, "YYYY-MM-DD HH:mm:ss");

      expect(result).toMatch(/\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}/);
    });

    it("should format with day name", () => {
      const dateTime = "2024-01-15T14:30:00Z";
      const result = formatDateTime(dateTime, "dddd, MMMM Do YYYY");

      expect(result).toMatch(
        /[A-Z][a-z]+, [A-Z][a-z]+ \d{1,2}(st|nd|rd|th) \d{4}/
      );
    });

    it("should handle timezone conversions", () => {
      const dateTime = "2024-01-15T14:30:00Z";
      const result = formatDateTime(dateTime, "YYYY-MM-DD HH:mm Z");

      expect(result).toMatch(/\d{4}-\d{2}-\d{2} \d{2}:\d{2} [+-]\d{2}:\d{2}/);
    });

    it("should handle relative time format", () => {
      const dateTime = moment().subtract(2, "hours").toISOString();
      const result = formatDateTime(dateTime, "YYYY-MM-DD HH:mm");

      expect(result).toMatch(/\d{4}-\d{2}-\d{2} \d{2}:\d{2}/);
    });
  });

  describe("formatSecondsForTimer", () => {
    it("should format 0 seconds as 00:00", () => {
      expect(formatSecondsForTimer(0)).toBe("00:00");
    });

    it("should format seconds less than 60", () => {
      expect(formatSecondsForTimer(30)).toBe("00:30");
      expect(formatSecondsForTimer(45)).toBe("00:45");
      expect(formatSecondsForTimer(59)).toBe("00:59");
    });

    it("should format exactly 60 seconds as 01:00", () => {
      expect(formatSecondsForTimer(60)).toBe("01:00");
    });

    it("should format minutes and seconds correctly", () => {
      expect(formatSecondsForTimer(90)).toBe("01:30");
      expect(formatSecondsForTimer(125)).toBe("02:05");
      expect(formatSecondsForTimer(305)).toBe("05:05");
    });

    it("should pad single digit seconds with zero", () => {
      expect(formatSecondsForTimer(61)).toBe("01:01");
      expect(formatSecondsForTimer(120)).toBe("02:00");
      expect(formatSecondsForTimer(125)).toBe("02:05");
    });

    it("should pad single digit minutes with zero", () => {
      expect(formatSecondsForTimer(540)).toBe("09:00");
      expect(formatSecondsForTimer(65)).toBe("01:05");
    });

    it("should handle double digit minutes", () => {
      expect(formatSecondsForTimer(600)).toBe("10:00");
      expect(formatSecondsForTimer(725)).toBe("12:05");
      expect(formatSecondsForTimer(3599)).toBe("59:59");
    });

    it("should handle large numbers (over an hour)", () => {
      expect(formatSecondsForTimer(3600)).toBe("60:00");
      expect(formatSecondsForTimer(3661)).toBe("61:01");
      expect(formatSecondsForTimer(7200)).toBe("120:00");
    });

    it("should handle 1 second", () => {
      expect(formatSecondsForTimer(1)).toBe("00:01");
    });

    it("should handle 10 seconds", () => {
      expect(formatSecondsForTimer(10)).toBe("00:10");
    });

    it("should format common countdown times", () => {
      expect(formatSecondsForTimer(300)).toBe("05:00"); // 5 minutes
      expect(formatSecondsForTimer(600)).toBe("10:00"); // 10 minutes
      expect(formatSecondsForTimer(900)).toBe("15:00"); // 15 minutes
      expect(formatSecondsForTimer(1800)).toBe("30:00"); // 30 minutes
    });
  });
});

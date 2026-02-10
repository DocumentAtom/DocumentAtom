import {
  toTitleCase,
  getFirstLetterOfTheWord,
  uuid,
  decodePayload,
} from "#/utils/stringUtils";
import { getMessageApi } from "#/utils/messageHolder";

// Mock messageHolder
jest.mock("#/utils/messageHolder", () => ({
  getMessageApi: jest.fn(() => ({
    error: jest.fn(),
  })),
}));

// Mock uuid
jest.mock("uuid", () => ({
  v4: jest.fn(() => "mocked-uuid-1234"),
}));

describe("stringUtils", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("toTitleCase", () => {
    it("should convert lowercase string to title case", () => {
      expect(toTitleCase("hello world")).toBe("Hello World");
    });

    it("should convert uppercase string to title case", () => {
      expect(toTitleCase("HELLO WORLD")).toBe("Hello World");
    });

    it("should convert mixed case string to title case", () => {
      expect(toTitleCase("hElLo WoRlD")).toBe("Hello World");
    });

    it("should replace hyphens with spaces and convert to title case", () => {
      expect(toTitleCase("hello-world")).toBe("Hello World");
    });

    it("should handle multiple hyphens", () => {
      expect(toTitleCase("hello-world-test-case")).toBe(
        "Hello World Test Case"
      );
    });

    it("should handle single word", () => {
      expect(toTitleCase("hello")).toBe("Hello");
    });

    it("should handle empty string", () => {
      expect(toTitleCase("")).toBe("");
    });

    it("should handle string with multiple spaces", () => {
      expect(toTitleCase("hello  world")).toBe("Hello  World");
    });

    it("should handle string with trailing/leading spaces", () => {
      expect(toTitleCase("  hello world  ")).toBe("  Hello World  ");
    });

    it("should handle string with numbers", () => {
      expect(toTitleCase("hello123world")).toBe("Hello123world");
    });

    it("should handle string with special characters", () => {
      expect(toTitleCase("hello@world")).toBe("Hello@world");
    });
  });

  describe("getFirstLetterOfTheWord", () => {
    it("should return first letter uppercased", () => {
      expect(getFirstLetterOfTheWord("hello")).toBe("H");
    });

    it("should return first letter uppercased from lowercase", () => {
      expect(getFirstLetterOfTheWord("test")).toBe("T");
    });

    it("should keep uppercase letter uppercase", () => {
      expect(getFirstLetterOfTheWord("Hello")).toBe("H");
    });

    it("should handle single character", () => {
      expect(getFirstLetterOfTheWord("a")).toBe("A");
    });

    it("should handle empty string", () => {
      expect(getFirstLetterOfTheWord("")).toBe("");
    });

    it("should handle undefined", () => {
      expect(getFirstLetterOfTheWord(undefined as any)).toBe("");
    });

    it("should handle null", () => {
      expect(getFirstLetterOfTheWord(null as any)).toBe("");
    });

    it("should handle number as first character", () => {
      expect(getFirstLetterOfTheWord("123abc")).toBe("1");
    });

    it("should handle special character as first character", () => {
      expect(getFirstLetterOfTheWord("@hello")).toBe("@");
    });

    it("should handle space as first character", () => {
      expect(getFirstLetterOfTheWord(" hello")).toBe(" ");
    });
  });

  describe("uuid", () => {
    it("should return a uuid", () => {
      const result = uuid();
      expect(result).toBe("mocked-uuid-1234");
    });

    it("should call v4 function", () => {
      const { v4 } = require("uuid");
      uuid();
      expect(v4).toHaveBeenCalled();
    });

    it("should return unique values on multiple calls", () => {
      const { v4 } = require("uuid");
      v4.mockReturnValueOnce("uuid-1")
        .mockReturnValueOnce("uuid-2")
        .mockReturnValueOnce("uuid-3");

      expect(uuid()).toBe("uuid-1");
      expect(uuid()).toBe("uuid-2");
      expect(uuid()).toBe("uuid-3");
    });
  });

  describe("decodePayload", () => {
    it("should decode valid base64 encoded JSON", () => {
      const data = { name: "test", value: 123 };
      const encoded = btoa(JSON.stringify(data));

      const result = decodePayload(encoded);
      expect(result).toEqual(data);
    });

    it("should handle simple string payload", () => {
      const data = "hello";
      const encoded = btoa(JSON.stringify(data));

      const result = decodePayload(encoded);
      expect(result).toBe(data);
    });

    it("should handle complex nested object", () => {
      const data = {
        user: {
          name: "John",
          age: 30,
          address: {
            city: "New York",
            zip: "10001",
          },
        },
      };
      const encoded = btoa(JSON.stringify(data));

      const result = decodePayload(encoded);
      expect(result).toEqual(data);
    });

    it("should handle array payload", () => {
      const data = [1, 2, 3, 4, 5];
      const encoded = btoa(JSON.stringify(data));

      const result = decodePayload(encoded);
      expect(result).toEqual(data);
    });

    it("should handle null payload", () => {
      const data = null;
      const encoded = btoa(JSON.stringify(data));

      const result = decodePayload(encoded);
      expect(result).toBeNull();
    });

    it("should return original payload and show error for invalid base64", () => {
      const mockError = jest.fn();
      (getMessageApi as jest.Mock).mockReturnValue({ error: mockError });
      const invalidPayload = "not-base64-encoded!!!";

      const result = decodePayload(invalidPayload);

      expect(result).toBe(invalidPayload);
      expect(mockError).toHaveBeenCalledWith("Failed to decode payload.");
    });

    it("should return original payload and show error for invalid JSON", () => {
      const mockError = jest.fn();
      (getMessageApi as jest.Mock).mockReturnValue({ error: mockError });
      const invalidJson = btoa("not valid json {");

      const result = decodePayload(invalidJson);

      expect(result).toBe(invalidJson);
      expect(mockError).toHaveBeenCalledWith("Failed to decode payload.");
    });

    it("should handle empty string", () => {
      const encoded = btoa(JSON.stringify(""));

      const result = decodePayload(encoded);
      expect(result).toBe("");
    });

    it("should handle boolean values", () => {
      const trueEncoded = btoa(JSON.stringify(true));
      const falseEncoded = btoa(JSON.stringify(false));

      expect(decodePayload(trueEncoded)).toBe(true);
      expect(decodePayload(falseEncoded)).toBe(false);
    });

    it("should handle number values", () => {
      const encoded = btoa(JSON.stringify(42));

      const result = decodePayload(encoded);
      expect(result).toBe(42);
    });
  });
});

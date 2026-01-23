import { getDashboardPathKey, transformToOptions } from "#/utils/appUtils";

describe("appUtils", () => {
  describe("getDashboardPathKey", () => {
    it("should extract path key from nested path", () => {
      const result = getDashboardPathKey("/dashboard/settings/profile");
      expect(result).toEqual({
        pathKey: "profile",
        patentPathKey: "settings",
      });
    });

    it("should handle root dashboard path", () => {
      const result = getDashboardPathKey("/dashboard");
      expect(result).toEqual({ pathKey: "dashboard", patentPathKey: "" });
    });

    it("should handle path with GUID and return 'dashboard'", () => {
      const guid = "123e4567-e89b-12d3-a456-426614174000";
      const result = getDashboardPathKey(`/dashboard/${guid}`);
      expect(result).toEqual({ pathKey: "dashboard", patentPathKey: "" });
    });

    it("should handle nested path with GUID", () => {
      const guid = "123e4567-e89b-12d3-a456-426614174000";
      const result = getDashboardPathKey(`/dashboard/users/${guid}`);
      expect(result).toEqual({ pathKey: "dashboard", patentPathKey: "users" });
    });

    it("should handle empty path", () => {
      const result = getDashboardPathKey("");
      expect(result).toEqual({ pathKey: "", patentPathKey: "" });
    });

    it("should handle single slash", () => {
      const result = getDashboardPathKey("/");
      expect(result).toEqual({ pathKey: "", patentPathKey: "" });
    });

    it("should handle path without dashboard", () => {
      const result = getDashboardPathKey("/settings/profile");
      expect(result).toEqual({ pathKey: "profile", patentPathKey: "settings" });
    });

    it("should handle multiple GUIDs in path", () => {
      const guid1 = "123e4567-e89b-12d3-a456-426614174000";
      const guid2 = "223e4567-e89b-12d3-a456-426614174001";
      const result = getDashboardPathKey(`/dashboard/${guid1}/${guid2}`);
      expect(result).toEqual({ pathKey: "dashboard", patentPathKey: "" });
    });

    it("should handle path with trailing slash", () => {
      const result = getDashboardPathKey("/dashboard/users/");
      expect(result).toEqual({ pathKey: "", patentPathKey: "users" });
    });

    it("should handle complex nested paths", () => {
      const result = getDashboardPathKey("/dashboard/admin/users/settings");
      expect(result).toEqual({
        pathKey: "settings",
        patentPathKey: "users",
      });
    });

    it("should handle uppercase GUID", () => {
      const guid = "123E4567-E89B-12D3-A456-426614174000";
      const result = getDashboardPathKey(`/dashboard/${guid}`);
      expect(result).toEqual({ pathKey: "dashboard", patentPathKey: "" });
    });

    it("should handle path with numbers that aren't GUIDs", () => {
      const result = getDashboardPathKey("/dashboard/user/123");
      expect(result).toEqual({ pathKey: "123", patentPathKey: "user" });
    });
  });

  describe("transformToOptions", () => {
    interface TestItem {
      GUID: string;
      name: string;
      Name?: string;
    }

    it("should transform array to options with lowercase name", () => {
      const data: TestItem[] = [
        { GUID: "guid-1", name: "Item 1" },
        { GUID: "guid-2", name: "Item 2" },
      ];

      const result = transformToOptions(data);

      expect(result).toEqual([
        { value: "guid-1", label: "Item 1" },
        { value: "guid-2", label: "Item 2" },
      ]);
    });

    it("should transform array to options with uppercase Name", () => {
      const data: any[] = [
        { GUID: "guid-1", Name: "Item 1" },
        { GUID: "guid-2", Name: "Item 2" },
      ];

      const result = transformToOptions(data);

      expect(result).toEqual([
        { value: "guid-1", label: "Item 1" },
        { value: "guid-2", label: "Item 2" },
      ]);
    });

    it("should use custom label field", () => {
      const data: any[] = [
        { GUID: "guid-1", title: "Title 1", name: "Name 1" },
        { GUID: "guid-2", title: "Title 2", name: "Name 2" },
      ];

      const result = transformToOptions(data, "title");

      expect(result).toEqual([
        { value: "guid-1", label: "Title 1" },
        { value: "guid-2", label: "Title 2" },
      ]);
    });

    it("should fallback to Name if label field is not found", () => {
      const data: any[] = [
        { GUID: "guid-1", Name: "Name 1" },
        { GUID: "guid-2", Name: "Name 2" },
      ];

      const result = transformToOptions(data, "missingField" as any);

      expect(result).toEqual([
        { value: "guid-1", label: "Name 1" },
        { value: "guid-2", label: "Name 2" },
      ]);
    });

    it("should fallback to GUID if no name fields are found", () => {
      const data: any[] = [{ GUID: "guid-1" }, { GUID: "guid-2" }];

      const result = transformToOptions(data);

      expect(result).toEqual([
        { value: "guid-1", label: "guid-1" },
        { value: "guid-2", label: "guid-2" },
      ]);
    });

    it("should handle empty array", () => {
      const result = transformToOptions([]);
      expect(result).toEqual([]);
    });

    it("should handle null input", () => {
      const result = transformToOptions(null);
      expect(result).toEqual([]);
    });

    it("should handle undefined input", () => {
      const result = transformToOptions(undefined);
      expect(result).toEqual([]);
    });

    it("should handle single item array", () => {
      const data: TestItem[] = [{ GUID: "guid-1", name: "Item 1" }];

      const result = transformToOptions(data);

      expect(result).toEqual([{ value: "guid-1", label: "Item 1" }]);
    });

    it("should handle items with empty names", () => {
      const data: TestItem[] = [
        { GUID: "guid-1", name: "" },
        { GUID: "guid-2", name: "Item 2" },
      ];

      const result = transformToOptions(data);

      expect(result).toEqual([
        { value: "guid-1", label: "guid-1" }, // Falls back to GUID
        { value: "guid-2", label: "Item 2" },
      ]);
    });

    it("should handle large datasets", () => {
      const data: TestItem[] = Array.from({ length: 1000 }, (_, i) => ({
        GUID: `guid-${i}`,
        name: `Item ${i}`,
      }));

      const result = transformToOptions(data);

      expect(result).toHaveLength(1000);
      expect(result[0]).toEqual({ value: "guid-0", label: "Item 0" });
      expect(result[999]).toEqual({ value: "guid-999", label: "Item 999" });
    });

    it("should handle special characters in names", () => {
      const data: TestItem[] = [
        { GUID: "guid-1", name: "Item !@#$%^&*()" },
        { GUID: "guid-2", name: "Item with <html>" },
      ];

      const result = transformToOptions(data);

      expect(result).toEqual([
        { value: "guid-1", label: "Item !@#$%^&*()" },
        { value: "guid-2", label: "Item with <html>" },
      ]);
    });

    it("should handle unicode characters in names", () => {
      const data: TestItem[] = [
        { GUID: "guid-1", name: "项目 1" },
        { GUID: "guid-2", name: "مشروع 2" },
      ];

      const result = transformToOptions(data);

      expect(result).toEqual([
        { value: "guid-1", label: "项目 1" },
        { value: "guid-2", label: "مشروع 2" },
      ]);
    });

    it("should preserve GUID format", () => {
      const guid = "123e4567-e89b-12d3-a456-426614174000";
      const data: TestItem[] = [{ GUID: guid, name: "Item 1" }];

      const result = transformToOptions(data);

      expect(result[0].value).toBe(guid);
    });

    it("should handle mixed Name and name fields", () => {
      const data: any[] = [
        { GUID: "guid-1", name: "lowercase name", Name: "Uppercase Name" },
        { GUID: "guid-2", name: "another lowercase" },
      ];

      const result = transformToOptions(data);

      expect(result).toEqual([
        { value: "guid-1", label: "lowercase name" }, // Prefers lowercase name
        { value: "guid-2", label: "another lowercase" },
      ]);
    });
  });
});

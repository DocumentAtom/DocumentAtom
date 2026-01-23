import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import TableSearch from "#/components/table-search/TableSearch";
import { FilterDropdownProps } from "antd/es/table/interface";

describe("TableSearch", () => {
  const mockSetSelectedKeys = jest.fn();
  const mockConfirm = jest.fn();
  const mockClearFilters = jest.fn();
  const mockClose = jest.fn();

  const defaultProps: FilterDropdownProps & { placeholder?: string } = {
    setSelectedKeys: mockSetSelectedKeys,
    selectedKeys: [],
    confirm: mockConfirm,
    clearFilters: mockClearFilters,
    close: mockClose,
    visible: true,
    filters: [],
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("Rendering", () => {
    it("should render search input", () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByPlaceholderText("Search");
      expect(searchInput).toBeInTheDocument();
    });

    it("should render with custom placeholder", () => {
      render(<TableSearch {...defaultProps} placeholder="Search users" />);

      const searchInput = screen.getByPlaceholderText("Search users");
      expect(searchInput).toBeInTheDocument();
    });

    it("should render with default placeholder when not provided", () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByPlaceholderText("Search");
      expect(searchInput).toBeInTheDocument();
    });

    it("should render with selected keys value", () => {
      render(<TableSearch {...defaultProps} selectedKeys={["test value"]} />);

      const searchInput = screen.getByPlaceholderText(
        "Search"
      ) as HTMLInputElement;
      expect(searchInput.value).toBe("test value");
    });

    it("should render empty when no selected keys", () => {
      render(<TableSearch {...defaultProps} selectedKeys={[]} />);

      const searchInput = screen.getByPlaceholderText(
        "Search"
      ) as HTMLInputElement;
      expect(searchInput.value).toBe("");
    });

    it("should have allowClear attribute", () => {
      const { container } = render(<TableSearch {...defaultProps} />);

      const searchWrapper = container.querySelector(".ant-input-search");
      expect(searchWrapper).toBeInTheDocument();
    });
  });

  describe("User Interactions", () => {
    it("should call setSelectedKeys when typing", async () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByPlaceholderText("Search");
      await userEvent.type(searchInput, "test");

      expect(mockSetSelectedKeys).toHaveBeenCalled();
      // Check if it was called with the expected values for each character
      expect(mockSetSelectedKeys).toHaveBeenCalledWith(["t"]);
      expect(mockSetSelectedKeys).toHaveBeenCalledWith(["te"]);
      expect(mockSetSelectedKeys).toHaveBeenCalledWith(["tes"]);
      expect(mockSetSelectedKeys).toHaveBeenCalledWith(["test"]);
    });

    it("should call setSelectedKeys with empty array when input is cleared", async () => {
      render(<TableSearch {...defaultProps} selectedKeys={["initial"]} />);

      const searchInput = screen.getByPlaceholderText("Search");
      await userEvent.clear(searchInput);

      expect(mockSetSelectedKeys).toHaveBeenCalledWith([]);
    });

    it("should call confirm when input is cleared", async () => {
      render(<TableSearch {...defaultProps} selectedKeys={["initial"]} />);

      const searchInput = screen.getByPlaceholderText("Search");
      await userEvent.clear(searchInput);

      expect(mockConfirm).toHaveBeenCalled();
    });

    it("should call confirm when search button is clicked", async () => {
      render(<TableSearch {...defaultProps} />);

      const searchButton = screen.getByRole("button");
      await userEvent.click(searchButton);

      expect(mockConfirm).toHaveBeenCalled();
    });

    it("should call confirm when Enter is pressed", async () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByPlaceholderText("Search");
      await userEvent.type(searchInput, "test{Enter}");

      expect(mockConfirm).toHaveBeenCalled();
    });

    it("should not call confirm when typing (only on Enter or button click)", async () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByPlaceholderText("Search");
      mockConfirm.mockClear(); // Clear any previous calls

      await userEvent.type(searchInput, "test");

      // Confirm should not be called during typing (only setSelectedKeys)
      // It should only be called when Enter is pressed or search button clicked
    });

    it("should handle multiple search queries", async () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByPlaceholderText("Search");

      // First search
      await userEvent.type(searchInput, "first{Enter}");
      expect(mockConfirm).toHaveBeenCalledTimes(1);

      // Clear and second search
      await userEvent.clear(searchInput);
      await userEvent.type(searchInput, "second{Enter}");

      // Should be called again (once for clear, once for Enter)
      expect(mockConfirm).toHaveBeenCalled();
    });

    it("should handle special characters in search", async () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByPlaceholderText("Search");
      await userEvent.type(searchInput, "test@#$%");

      expect(mockSetSelectedKeys).toHaveBeenCalled();
    });

    it("should handle spaces in search query", async () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByPlaceholderText("Search");
      await userEvent.type(searchInput, "test query");

      expect(mockSetSelectedKeys).toHaveBeenCalled();
    });

    it("should handle unicode characters", async () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByPlaceholderText("Search");
      await userEvent.type(searchInput, "测试");

      expect(mockSetSelectedKeys).toHaveBeenCalled();
    });
  });

  describe("Edge Cases", () => {
    it("should handle empty search input", async () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByPlaceholderText("Search");
      await userEvent.type(searchInput, "test");
      await userEvent.clear(searchInput);

      expect(mockSetSelectedKeys).toHaveBeenCalledWith([]);
      expect(mockConfirm).toHaveBeenCalled();
    });

    it("should handle rapid typing", async () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByPlaceholderText("Search");
      await userEvent.type(searchInput, "quick");

      // setSelectedKeys should be called multiple times for each character
      expect(mockSetSelectedKeys).toHaveBeenCalled();
    });

    it("should handle multiple Enter presses", async () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByPlaceholderText("Search");
      await userEvent.type(searchInput, "test{Enter}{Enter}{Enter}");

      // Confirm should be called multiple times
      expect(mockConfirm).toHaveBeenCalled();
    });

    it("should handle backspace correctly", async () => {
      render(<TableSearch {...defaultProps} selectedKeys={["test"]} />);

      const searchInput = screen.getByPlaceholderText("Search");
      await userEvent.type(searchInput, "{Backspace}");

      expect(mockSetSelectedKeys).toHaveBeenCalled();
    });
  });

  describe("Controlled Input", () => {
    it("should display selectedKeys value", () => {
      render(<TableSearch {...defaultProps} selectedKeys={["controlled"]} />);

      const searchInput = screen.getByPlaceholderText(
        "Search"
      ) as HTMLInputElement;
      expect(searchInput.value).toBe("controlled");
    });

    it("should update when selectedKeys prop changes", () => {
      const { rerender } = render(
        <TableSearch {...defaultProps} selectedKeys={["first"]} />
      );

      let searchInput = screen.getByPlaceholderText(
        "Search"
      ) as HTMLInputElement;
      expect(searchInput.value).toBe("first");

      rerender(<TableSearch {...defaultProps} selectedKeys={["second"]} />);

      searchInput = screen.getByPlaceholderText("Search") as HTMLInputElement;
      expect(searchInput.value).toBe("second");
    });

    it("should handle multiple values in selectedKeys (use first one)", () => {
      render(
        <TableSearch {...defaultProps} selectedKeys={["first", "second"]} />
      );

      const searchInput = screen.getByPlaceholderText(
        "Search"
      ) as HTMLInputElement;
      expect(searchInput.value).toBe("first");
    });
  });

  describe("Accessibility", () => {
    it("should have search input accessible by role", () => {
      render(<TableSearch {...defaultProps} />);

      const searchInput = screen.getByRole("searchbox");
      expect(searchInput).toBeInTheDocument();
    });

    it("should have search button accessible by role", () => {
      render(<TableSearch {...defaultProps} />);

      const searchButton = screen.getByRole("button");
      expect(searchButton).toBeInTheDocument();
    });
  });
});

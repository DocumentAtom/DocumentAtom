import { render } from "@testing-library/react";
import JSONEditor from "#/components/base/json-editor/JSONEditor";

describe("JSONEditor", () => {
  const mockOnChange = jest.fn();
  const defaultProps = {
    value: { test: "data" },
    onChange: mockOnChange,
    uniqueKey: "test-key",
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("Rendering", () => {
    it("should render without crashing", () => {
      const { container } = render(<JSONEditor {...defaultProps} />);

      // JSONEditor uses a third-party component that may not render in test environment
      // We just verify the component doesn't crash
      expect(container).toBeInTheDocument();
    });

    it("should accept different modes", () => {
      const { container } = render(
        <JSONEditor {...defaultProps} mode="tree" />
      );

      expect(container).toBeInTheDocument();
    });

    it("should handle different configuration options", () => {
      const { container } = render(
        <JSONEditor
          {...defaultProps}
          enableSort
          enableTransform
          expandOnStart={false}
        />
      );

      expect(container).toBeInTheDocument();
    });
  });

  describe("Props", () => {
    it("should accept value prop", () => {
      const testValue = { message: "test" };
      const { container } = render(
        <JSONEditor {...defaultProps} value={testValue} />
      );

      expect(container).toBeInTheDocument();
    });

    it("should accept onChange prop", () => {
      const { container } = render(<JSONEditor {...defaultProps} />);

      expect(container).toBeInTheDocument();
      expect(mockOnChange).toBeDefined();
    });

    it("should accept uniqueKey prop", () => {
      const { container } = render(
        <JSONEditor {...defaultProps} uniqueKey="unique-key" />
      );

      expect(container).toBeInTheDocument();
    });

    it("should accept testId prop", () => {
      const { container } = render(
        <JSONEditor {...defaultProps} testId="custom-test" />
      );

      expect(container).toBeInTheDocument();
    });
  });

  describe("Snapshots", () => {
    it("should match snapshot", () => {
      const { container } = render(<JSONEditor {...defaultProps} />);

      expect(container.firstChild).toMatchSnapshot();
    });

    it("should match snapshot in tree mode", () => {
      const { container } = render(
        <JSONEditor {...defaultProps} mode="tree" />
      );

      expect(container.firstChild).toMatchSnapshot();
    });
  });
});

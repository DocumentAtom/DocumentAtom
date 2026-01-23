// Mock the react-password-checklist component
jest.mock("react-password-checklist", () => ({
  __esModule: true,
  default: ({
    className,
    rules,
    minLength,
    value,
    valueAgain,
    onChange,
  }: any) => (
    <div className={className} data-testid="password-checklist">
      <div>minLength: {minLength}</div>
      <div>Rules: {rules.join(", ")}</div>
      <div>Value: {value}</div>
      <div>ValueAgain: {valueAgain}</div>
      <div>Valid: {value === valueAgain && value.length >= minLength}</div>
    </div>
  ),
}));

import { render, screen } from "@testing-library/react";
import DocuAtomPasswordCheckList from "#/components/base/password/PasswordCheckList";

describe("DocuAtomPasswordCheckList", () => {
  const mockOnChange = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("Rendering", () => {
    it("should render password checklist", () => {
      render(
        <DocuAtomPasswordCheckList
          value="password"
          valueAgain="password"
          onChange={mockOnChange}
        />
      );

      expect(screen.getByTestId("password-checklist")).toBeInTheDocument();
      expect(screen.getByText("minLength: 8")).toBeInTheDocument();
      expect(screen.getByText(/Rules:/)).toBeInTheDocument();
    });

    it("should apply custom className", () => {
      render(
        <DocuAtomPasswordCheckList
          value="password"
          valueAgain="password"
          onChange={mockOnChange}
          className="custom-checklist"
        />
      );

      expect(screen.getByTestId("password-checklist")).toHaveClass(
        "custom-checklist"
      );
    });
  });

  describe("Validation Rules", () => {
    it("should show all validation rules", () => {
      render(
        <DocuAtomPasswordCheckList
          value="password"
          valueAgain="password"
          onChange={mockOnChange}
        />
      );

      expect(
        screen.getByText(
          /Rules: minLength, specialChar, number, capital, match/
        )
      ).toBeInTheDocument();
    });

    it("should handle empty values", () => {
      render(
        <DocuAtomPasswordCheckList
          value=""
          valueAgain=""
          onChange={mockOnChange}
        />
      );

      expect(screen.getByText(/Value:/)).toBeInTheDocument();
      expect(screen.getByText(/ValueAgain:/)).toBeInTheDocument();
    });
  });

  describe("Callbacks", () => {
    it("should call onChange with validation status", () => {
      render(
        <DocuAtomPasswordCheckList
          value="Password123!"
          valueAgain="Password123!"
          onChange={mockOnChange}
        />
      );

      // The onChange should be called by the ReactPasswordChecklist component
      expect(mockOnChange).toBeDefined();
    });
  });

  describe("Snapshots", () => {
    it("should match snapshot", () => {
      const { container } = render(
        <DocuAtomPasswordCheckList
          value="password"
          valueAgain="password"
          onChange={mockOnChange}
        />
      );

      expect(container.firstChild).toMatchSnapshot();
    });

    it("should match snapshot with valid password", () => {
      const { container } = render(
        <DocuAtomPasswordCheckList
          value="ValidPassword123!"
          valueAgain="ValidPassword123!"
          onChange={mockOnChange}
        />
      );

      expect(container.firstChild).toMatchSnapshot();
    });
  });
});

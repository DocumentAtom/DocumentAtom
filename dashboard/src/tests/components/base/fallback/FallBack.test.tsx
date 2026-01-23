import { render, screen, fireEvent } from "@testing-library/react";
import FallBack, { FallBackEnums } from "#/components/base/fallback/FallBack";

describe("FallBack", () => {
  describe("Rendering", () => {
    it("should render error fallback by default", () => {
      const { container } = render(<FallBack />);

      expect(screen.getByText("Something went wrong.")).toBeInTheDocument();
      expect(
        container.querySelector(".anticon-close-circle")
      ).toBeInTheDocument();
    });

    it("should render custom message", () => {
      render(<FallBack>Custom error message</FallBack>);

      expect(screen.getByText("Custom error message")).toBeInTheDocument();
    });

    it("should render warning type", () => {
      const { container } = render(<FallBack type={FallBackEnums.WARNING} />);

      expect(container.querySelector(".anticon-warning")).toBeInTheDocument();
    });

    it("should render info type", () => {
      const { container } = render(<FallBack type={FallBackEnums.INFO} />);

      expect(
        container.querySelector(".anticon-info-circle")
      ).toBeInTheDocument();
    });

    it("should render custom icon", () => {
      const customIcon = <span data-testid="custom-icon">🔥</span>;
      render(<FallBack icon={customIcon} />);

      expect(screen.getByTestId("custom-icon")).toBeInTheDocument();
    });
  });

  describe("Retry Functionality", () => {
    it("should render retry button when retry prop is provided", () => {
      const mockRetry = jest.fn();
      render(<FallBack retry={mockRetry} />);

      expect(screen.getByText("Retry")).toBeInTheDocument();
    });

    it("should call retry function when retry is clicked", () => {
      const mockRetry = jest.fn();
      render(<FallBack retry={mockRetry} />);

      fireEvent.click(screen.getByText("Retry"));
      expect(mockRetry).toHaveBeenCalledTimes(1);
    });

    it("should not render retry button when retry prop is not provided", () => {
      render(<FallBack />);

      expect(screen.queryByText("Retry")).not.toBeInTheDocument();
    });
  });

  describe("Styling", () => {
    it("should apply custom className", () => {
      const { container } = render(<FallBack className="custom-class" />);

      expect(container.querySelector(".custom-class")).toBeInTheDocument();
    });

    it("should apply custom style", () => {
      const { container } = render(
        <FallBack style={{ backgroundColor: "red" }} />
      );

      expect(container.firstChild).toHaveStyle({ backgroundColor: "red" });
    });
  });

  describe("Snapshots", () => {
    it("should match snapshot for error type", () => {
      const { container } = render(<FallBack />);

      expect(container.firstChild).toMatchSnapshot();
    });

    it("should match snapshot for warning type", () => {
      const { container } = render(<FallBack type={FallBackEnums.WARNING} />);

      expect(container.firstChild).toMatchSnapshot();
    });

    it("should match snapshot for info type", () => {
      const { container } = render(<FallBack type={FallBackEnums.INFO} />);

      expect(container.firstChild).toMatchSnapshot();
    });
  });
});

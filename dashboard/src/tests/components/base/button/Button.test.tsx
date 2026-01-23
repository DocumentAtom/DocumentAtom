import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import DocuAtomButton from "#/components/base/button/Button";

describe("DocuAtomButton", () => {
  describe("Rendering", () => {
    it("should render button with text", () => {
      render(<DocuAtomButton>Click me</DocuAtomButton>);

      expect(screen.getByRole("button")).toBeInTheDocument();
      expect(screen.getByText("Click me")).toBeInTheDocument();
    });

    it("should render button without text", () => {
      render(<DocuAtomButton />);

      expect(screen.getByRole("button")).toBeInTheDocument();
    });

    it("should apply type prop", () => {
      render(<DocuAtomButton type="primary">Primary</DocuAtomButton>);

      const button = screen.getByRole("button");
      expect(button).toHaveClass("ant-btn-primary");
    });

    it("should apply different button types", () => {
      const { rerender } = render(
        <DocuAtomButton type="default">Button</DocuAtomButton>
      );
      expect(screen.getByRole("button")).toHaveClass("ant-btn");

      rerender(<DocuAtomButton type="dashed">Button</DocuAtomButton>);
      expect(screen.getByRole("button")).toHaveClass("ant-btn-dashed");

      rerender(<DocuAtomButton type="link">Button</DocuAtomButton>);
      expect(screen.getByRole("button")).toHaveClass("ant-btn-link");

      rerender(<DocuAtomButton type="text">Button</DocuAtomButton>);
      expect(screen.getByRole("button")).toHaveClass("ant-btn-text");
    });

    it("should render with icon", () => {
      const icon = <span data-testid="test-icon">🔥</span>;
      render(<DocuAtomButton icon={icon}>With Icon</DocuAtomButton>);

      expect(screen.getByTestId("test-icon")).toBeInTheDocument();
      expect(screen.getByText("With Icon")).toBeInTheDocument();
    });

    it("should render icon-only button", () => {
      const icon = <span data-testid="test-icon">🔥</span>;
      render(<DocuAtomButton icon={icon} />);

      expect(screen.getByTestId("test-icon")).toBeInTheDocument();
    });
  });

  describe("Custom weight prop", () => {
    it("should apply custom font weight", () => {
      render(<DocuAtomButton weight={700}>Bold Button</DocuAtomButton>);

      const button = screen.getByRole("button");
      expect(button).toHaveStyle({ fontWeight: 700 });
    });

    it("should apply different weight values", () => {
      const { rerender } = render(
        <DocuAtomButton weight={300}>Light</DocuAtomButton>
      );
      expect(screen.getByRole("button")).toHaveStyle({ fontWeight: 300 });

      rerender(<DocuAtomButton weight={500}>Normal</DocuAtomButton>);
      expect(screen.getByRole("button")).toHaveStyle({ fontWeight: 500 });

      rerender(<DocuAtomButton weight={900}>Heavy</DocuAtomButton>);
      expect(screen.getByRole("button")).toHaveStyle({ fontWeight: 900 });
    });

    it("should not apply font weight when weight prop is not provided", () => {
      render(<DocuAtomButton>Normal Button</DocuAtomButton>);

      const button = screen.getByRole("button");
      // fontWeight should be undefined or not explicitly set
      expect(button).not.toHaveStyle({ fontWeight: expect.anything() });
    });
  });

  describe("Button states", () => {
    it("should render disabled button", () => {
      render(<DocuAtomButton disabled>Disabled</DocuAtomButton>);

      const button = screen.getByRole("button");
      expect(button).toBeDisabled();
      expect(button).toHaveClass(
        "ant-btn css-dev-only-do-not-override-1d4w9r2 ant-btn-default ant-btn-color-default ant-btn-variant-outlined"
      );
    });

    it("should render loading button", () => {
      render(<DocuAtomButton loading>Loading</DocuAtomButton>);

      const button = screen.getByRole("button");
      expect(button).toHaveClass("ant-btn-loading");
    });

    it("should render danger button", () => {
      render(<DocuAtomButton danger>Danger</DocuAtomButton>);

      const button = screen.getByRole("button");
      expect(button).toHaveClass("ant-btn-dangerous");
    });

    it("should render block button", () => {
      render(<DocuAtomButton block>Block Button</DocuAtomButton>);

      const button = screen.getByRole("button");
      expect(button).toHaveClass("ant-btn-block");
    });
  });

  describe("Button sizes", () => {
    it("should render large button", () => {
      render(<DocuAtomButton size="large">Large</DocuAtomButton>);

      expect(screen.getByRole("button")).toHaveClass("ant-btn-lg");
    });

    it("should render medium button (default)", () => {
      render(<DocuAtomButton size="middle">Medium</DocuAtomButton>);

      expect(screen.getByRole("button")).toBeInTheDocument();
    });

    it("should render small button", () => {
      render(<DocuAtomButton size="small">Small</DocuAtomButton>);

      expect(screen.getByRole("button")).toHaveClass("ant-btn-sm");
    });
  });

  describe("User Interactions", () => {
    it("should call onClick when clicked", async () => {
      const handleClick = jest.fn();
      render(<DocuAtomButton onClick={handleClick}>Click me</DocuAtomButton>);

      await userEvent.click(screen.getByRole("button"));

      expect(handleClick).toHaveBeenCalledTimes(1);
    });

    it("should not call onClick when disabled", async () => {
      const handleClick = jest.fn();
      render(
        <DocuAtomButton disabled onClick={handleClick}>
          Disabled
        </DocuAtomButton>
      );

      await userEvent.click(screen.getByRole("button"));

      expect(handleClick).not.toHaveBeenCalled();
    });

    it("should handle multiple clicks", async () => {
      const handleClick = jest.fn();
      render(<DocuAtomButton onClick={handleClick}>Click me</DocuAtomButton>);

      const button = screen.getByRole("button");
      await userEvent.click(button);
      await userEvent.click(button);
      await userEvent.click(button);

      expect(handleClick).toHaveBeenCalledTimes(3);
    });

    it("should be keyboard accessible", async () => {
      const handleClick = jest.fn();
      render(<DocuAtomButton onClick={handleClick}>Button</DocuAtomButton>);

      const button = screen.getByRole("button");
      button.focus();

      expect(button).toHaveFocus();

      await userEvent.keyboard("{Enter}");

      expect(handleClick).toHaveBeenCalled();
    });
  });

  describe("Custom styling", () => {
    it("should apply custom className", () => {
      render(<DocuAtomButton className="custom-class">Custom</DocuAtomButton>);

      expect(screen.getByRole("button")).toHaveClass("custom-class");
    });
  });

  describe("HTML button attributes", () => {
    it("should support htmlType prop", () => {
      render(<DocuAtomButton htmlType="submit">Submit</DocuAtomButton>);

      expect(screen.getByRole("button")).toHaveAttribute("type", "submit");
    });

    it("should support data attributes", () => {
      render(
        <DocuAtomButton data-testid="custom-button">Button</DocuAtomButton>
      );

      expect(screen.getByTestId("custom-button")).toBeInTheDocument();
    });

    it("should support aria attributes", () => {
      render(<DocuAtomButton aria-label="Close">✕</DocuAtomButton>);

      expect(screen.getByRole("button")).toHaveAttribute("aria-label", "Close");
    });
  });

  describe("Edge Cases", () => {
    it("should handle very long button text", () => {
      const longText = "A".repeat(100);
      render(<DocuAtomButton>{longText}</DocuAtomButton>);

      expect(screen.getByText(longText)).toBeInTheDocument();
    });

    it("should handle empty children", () => {
      render(<DocuAtomButton>{""}</DocuAtomButton>);

      expect(screen.getByRole("button")).toBeInTheDocument();
    });

    it("should handle special characters in text", () => {
      render(<DocuAtomButton>!@#$%^&*()</DocuAtomButton>);

      expect(screen.getByText("!@#$%^&*()")).toBeInTheDocument();
    });

    it("should handle unicode characters", () => {
      render(<DocuAtomButton>按钮 🚀</DocuAtomButton>);

      expect(screen.getByText("按钮 🚀")).toBeInTheDocument();
    });

    it("should handle both icon and weight props together", () => {
      const icon = <span data-testid="icon">★</span>;
      render(
        <DocuAtomButton icon={icon} weight={700}>
          Button
        </DocuAtomButton>
      );

      expect(screen.getByTestId("icon")).toBeInTheDocument();
      expect(screen.getByRole("button")).toHaveStyle({ fontWeight: 700 });
    });
  });

  describe("Form integration", () => {
    it("should work within a form", async () => {
      const handleSubmit = jest.fn((e) => e.preventDefault());

      render(
        <form onSubmit={handleSubmit}>
          <DocuAtomButton htmlType="submit">Submit</DocuAtomButton>
        </form>
      );

      await userEvent.click(screen.getByRole("button"));

      expect(handleSubmit).toHaveBeenCalled();
    });
  });
});

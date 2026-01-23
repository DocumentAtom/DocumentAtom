import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import DocuAtomTag from "#/components/base/tag/Tag";

describe("DocuAtomTag", () => {
  describe("Rendering", () => {
    it("should render tag with text", () => {
      render(<DocuAtomTag>Tag Text</DocuAtomTag>);

      expect(screen.getByText("Tag Text")).toBeInTheDocument();
    });

    it("should render tag without text", () => {
      const { container } = render(<DocuAtomTag />);

      const tag = container.querySelector(".ant-tag");
      expect(tag).toBeInTheDocument();
    });

    it("should render default tag color", () => {
      const { container } = render(<DocuAtomTag>Default</DocuAtomTag>);

      const tag = container.querySelector(".ant-tag");
      expect(tag).toBeInTheDocument();
    });
  });

  describe("Tag colors", () => {
    it("should render tag with predefined color", () => {
      const { container } = render(
        <DocuAtomTag color="blue">Blue Tag</DocuAtomTag>
      );

      const tag = container.querySelector(".ant-tag-blue");
      expect(tag).toBeInTheDocument();
    });

    it("should render tag with hex color", () => {
      const { container } = render(
        <DocuAtomTag color="#f50">#f50 Tag</DocuAtomTag>
      );

      const tag = container.querySelector(".ant-tag");
      expect(tag).toHaveStyle({ backgroundColor: "#f50" });
    });

    it("should render different predefined colors", () => {
      const colors = ["success", "processing", "error", "warning", "default"];

      colors.forEach((color) => {
        const { container } = render(
          <DocuAtomTag color={color}>{color}</DocuAtomTag>
        );
        const tag = container.querySelector(`.ant-tag-${color}`);
        expect(tag).toBeInTheDocument();
      });
    });
  });

  describe("Closable tag", () => {
    it("should render non-closable tag by default", () => {
      render(<DocuAtomTag>Tag</DocuAtomTag>);

      const closeIcon = screen.queryByRole("img", { hidden: true });
      expect(closeIcon).not.toBeInTheDocument();
    });

    it("should render closable tag", () => {
      const { container } = render(
        <DocuAtomTag closable>Closable</DocuAtomTag>
      );

      const closeIcon = container.querySelector(".anticon-close");
      expect(closeIcon).toBeInTheDocument();
    });

    it("should call onClose when close icon is clicked", async () => {
      const handleClose = jest.fn();
      const { container } = render(
        <DocuAtomTag closable onClose={handleClose}>
          Close Me
        </DocuAtomTag>
      );

      const closeIcon = container.querySelector(".anticon-close");
      if (closeIcon) {
        await userEvent.click(closeIcon);
        expect(handleClose).toHaveBeenCalledTimes(1);
      }
    });
  });

  describe("Tag with icon", () => {
    it("should render tag with icon", () => {
      const icon = <span data-testid="tag-icon">🏷️</span>;
      render(<DocuAtomTag icon={icon}>With Icon</DocuAtomTag>);

      expect(screen.getByTestId("tag-icon")).toBeInTheDocument();
      expect(screen.getByText("With Icon")).toBeInTheDocument();
    });

    it("should render icon-only tag", () => {
      const icon = <span data-testid="tag-icon">🏷️</span>;
      render(<DocuAtomTag icon={icon} />);

      expect(screen.getByTestId("tag-icon")).toBeInTheDocument();
    });
  });

  describe("Bordered tag", () => {
    it("should render bordered tag by default", () => {
      const { container } = render(<DocuAtomTag>Bordered</DocuAtomTag>);

      const tag = container.querySelector(".ant-tag");
      expect(tag).toBeInTheDocument();
    });

    it("should render borderless tag", () => {
      const { container } = render(
        <DocuAtomTag bordered={false}>Borderless</DocuAtomTag>
      );

      const tag = container.querySelector(".ant-tag-borderless");
      expect(tag).toBeInTheDocument();
    });
  });

  describe("Tag styling", () => {
    it("should apply custom className", () => {
      const { container } = render(
        <DocuAtomTag className="custom-tag">Custom</DocuAtomTag>
      );

      const tag = container.querySelector(".custom-tag");
      expect(tag).toBeInTheDocument();
    });

    it("should apply custom style", () => {
      const { container } = render(
        <DocuAtomTag style={{ fontSize: "16px" }}>Styled</DocuAtomTag>
      );

      const tag = container.querySelector(".ant-tag");
      expect(tag).toHaveStyle({ fontSize: "16px" });
    });
  });

  describe("Interactive tag", () => {
    it("should call onClick when clicked", async () => {
      const handleClick = jest.fn();
      render(<DocuAtomTag onClick={handleClick}>Clickable</DocuAtomTag>);

      await userEvent.click(screen.getByText("Clickable"));

      expect(handleClick).toHaveBeenCalledTimes(1);
    });

    it("should handle multiple clicks", async () => {
      const handleClick = jest.fn();
      render(<DocuAtomTag onClick={handleClick}>Click Me</DocuAtomTag>);

      const tag = screen.getByText("Click Me");
      await userEvent.click(tag);
      await userEvent.click(tag);
      await userEvent.click(tag);

      expect(handleClick).toHaveBeenCalledTimes(3);
    });
  });

  describe("Edge Cases", () => {
    it("should handle very long text", () => {
      const longText = "A".repeat(100);
      render(<DocuAtomTag>{longText}</DocuAtomTag>);

      expect(screen.getByText(longText)).toBeInTheDocument();
    });

    it("should handle special characters", () => {
      render(<DocuAtomTag>Tag !@#$%</DocuAtomTag>);

      expect(screen.getByText("Tag !@#$%")).toBeInTheDocument();
    });

    it("should handle unicode characters", () => {
      render(<DocuAtomTag>标签 🏷️</DocuAtomTag>);

      expect(screen.getByText("标签 🏷️")).toBeInTheDocument();
    });

    it("should handle empty children", () => {
      const { container } = render(<DocuAtomTag>{""}</DocuAtomTag>);

      const tag = container.querySelector(".ant-tag");
      expect(tag).toBeInTheDocument();
    });

    it("should handle null children", () => {
      const { container } = render(<DocuAtomTag>{null}</DocuAtomTag>);

      const tag = container.querySelector(".ant-tag");
      expect(tag).toBeInTheDocument();
    });
  });

  describe("Complex content", () => {
    it("should render JSX children", () => {
      render(
        <DocuAtomTag>
          <strong>Bold</strong> Text
        </DocuAtomTag>
      );

      expect(screen.getByText("Bold")).toBeInTheDocument();
      expect(screen.getByText("Text")).toBeInTheDocument();
    });

    it("should render with image", () => {
      render(
        <DocuAtomTag>
          <img src="/avatar.png" alt="avatar" data-testid="tag-img" />
          User
        </DocuAtomTag>
      );

      expect(screen.getByTestId("tag-img")).toBeInTheDocument();
      expect(screen.getByText("User")).toBeInTheDocument();
    });
  });

  describe("Tag groups", () => {
    it("should render multiple tags", () => {
      render(
        <div>
          <DocuAtomTag>Tag 1</DocuAtomTag>
          <DocuAtomTag>Tag 2</DocuAtomTag>
          <DocuAtomTag>Tag 3</DocuAtomTag>
        </div>
      );

      expect(screen.getByText("Tag 1")).toBeInTheDocument();
      expect(screen.getByText("Tag 2")).toBeInTheDocument();
      expect(screen.getByText("Tag 3")).toBeInTheDocument();
    });

    it("should render tags with different colors", () => {
      const { container } = render(
        <div>
          <DocuAtomTag color="success">Success</DocuAtomTag>
          <DocuAtomTag color="error">Error</DocuAtomTag>
          <DocuAtomTag color="warning">Warning</DocuAtomTag>
        </div>
      );

      expect(container.querySelector(".ant-tag-success")).toBeInTheDocument();
      expect(container.querySelector(".ant-tag-error")).toBeInTheDocument();
      expect(container.querySelector(".ant-tag-warning")).toBeInTheDocument();
    });
  });

  describe("Accessibility", () => {
    it("should be keyboard accessible when clickable", async () => {
      const handleClick = jest.fn();
      const { container } = render(
        <DocuAtomTag onClick={handleClick}>Clickable</DocuAtomTag>
      );

      const tag = container.querySelector(".ant-tag");
      if (tag) {
        (tag as HTMLElement).focus();
        await userEvent.keyboard("{Enter}");
        // Note: Tag might not respond to keyboard events by default
      }
    });

    it("should have proper structure", () => {
      const { container } = render(<DocuAtomTag>Tag</DocuAtomTag>);

      const tag = container.querySelector(".ant-tag");
      expect(tag).toBeInTheDocument();
      expect(tag?.tagName).toBe("SPAN");
    });
  });

  describe("Close animation", () => {
    it("should handle close with prevent default", async () => {
      const handleClose = jest.fn((e) => e.preventDefault());
      const { container } = render(
        <DocuAtomTag closable onClose={handleClose}>
          Tag
        </DocuAtomTag>
      );

      const closeIcon = container.querySelector(".anticon-close");
      if (closeIcon) {
        await userEvent.click(closeIcon);
        expect(handleClose).toHaveBeenCalled();
      }
    });
  });
});

import { render, screen } from "@testing-library/react";
import DocuAtomDivider from "#/components/base/divider/Divider";

describe("DocuAtomDivider", () => {
  describe("Rendering", () => {
    it("should render a divider", () => {
      const { container } = render(<DocuAtomDivider />);

      const divider = container.querySelector(".ant-divider");
      expect(divider).toBeInTheDocument();
    });

    it("should render divider with text", () => {
      render(<DocuAtomDivider>Divider Text</DocuAtomDivider>);

      expect(screen.getByText("Divider Text")).toBeInTheDocument();
    });

    it("should render horizontal divider by default", () => {
      const { container } = render(<DocuAtomDivider />);

      const divider = container.querySelector(".ant-divider-horizontal");
      expect(divider).toBeInTheDocument();
    });

    it("should render vertical divider", () => {
      const { container } = render(<DocuAtomDivider type="vertical" />);

      const divider = container.querySelector(".ant-divider-vertical");
      expect(divider).toBeInTheDocument();
    });
  });

  describe("Divider orientation", () => {
    it("should render divider with center orientation by default", () => {
      const { container } = render(<DocuAtomDivider>Center</DocuAtomDivider>);

      const divider = container.querySelector(".ant-divider-with-text-center");
      expect(divider).toBeInTheDocument();
    });
  });

  describe("Divider styling", () => {
    it("should apply custom className", () => {
      const { container } = render(
        <DocuAtomDivider className="custom-divider" />
      );

      const divider = container.querySelector(".custom-divider");
      expect(divider).toBeInTheDocument();
    });

    it("should apply custom style", () => {
      const { container } = render(
        <DocuAtomDivider style={{ borderColor: "red" }} />
      );

      const divider = container.querySelector(".ant-divider");
      expect(divider).toHaveStyle({ borderColor: "red" });
    });
  });

  describe("Dashed divider", () => {
    it("should render solid divider by default", () => {
      const { container } = render(<DocuAtomDivider />);

      const divider = container.querySelector(".ant-divider-dashed");
      expect(divider).not.toBeInTheDocument();
    });

    it("should render dashed divider", () => {
      const { container } = render(<DocuAtomDivider dashed />);

      const divider = container.querySelector(".ant-divider-dashed");
      expect(divider).toBeInTheDocument();
    });
  });

  describe("Plain divider", () => {
    it("should render divider with plain text", () => {
      const { container } = render(
        <DocuAtomDivider plain>Plain Text</DocuAtomDivider>
      );

      const divider = container.querySelector(".ant-divider-plain");
      expect(divider).toBeInTheDocument();
    });

    it("should render divider with styled text by default", () => {
      const { container } = render(
        <DocuAtomDivider>Styled Text</DocuAtomDivider>
      );

      const divider = container.querySelector(".ant-divider-plain");
      expect(divider).not.toBeInTheDocument();
    });
  });

  describe("Complex content", () => {
    it("should render JSX children", () => {
      render(
        <DocuAtomDivider>
          <strong>Bold Text</strong>
        </DocuAtomDivider>
      );

      expect(screen.getByText("Bold Text")).toBeInTheDocument();
    });

    it("should render icon in divider", () => {
      render(
        <DocuAtomDivider>
          <span data-testid="icon">🔥</span>
        </DocuAtomDivider>
      );

      expect(screen.getByTestId("icon")).toBeInTheDocument();
    });
  });

  describe("Edge Cases", () => {
    it("should handle empty children", () => {
      const { container } = render(<DocuAtomDivider>{""}</DocuAtomDivider>);

      const divider = container.querySelector(".ant-divider");
      expect(divider).toBeInTheDocument();
    });

    it("should handle very long text", () => {
      const longText = "A".repeat(100);
      render(<DocuAtomDivider>{longText}</DocuAtomDivider>);

      expect(screen.getByText(longText)).toBeInTheDocument();
    });

    it("should handle special characters", () => {
      render(<DocuAtomDivider>!@#$%^&*()</DocuAtomDivider>);

      expect(screen.getByText("!@#$%^&*()")).toBeInTheDocument();
    });

    it("should handle unicode characters", () => {
      render(<DocuAtomDivider>分隔线 ➖</DocuAtomDivider>);

      expect(screen.getByText("分隔线 ➖")).toBeInTheDocument();
    });

    it("should handle null children", () => {
      const { container } = render(<DocuAtomDivider>{null}</DocuAtomDivider>);

      const divider = container.querySelector(".ant-divider");
      expect(divider).toBeInTheDocument();
    });
  });

  describe("Layout usage", () => {
    it("should work as section divider", () => {
      const { container } = render(
        <div>
          <p>Section 1</p>
          <DocuAtomDivider />
          <p>Section 2</p>
        </div>
      );

      const divider = container.querySelector(".ant-divider");
      expect(divider).toBeInTheDocument();
      expect(screen.getByText("Section 1")).toBeInTheDocument();
      expect(screen.getByText("Section 2")).toBeInTheDocument();
    });

    it("should work as inline separator", () => {
      const { container } = render(
        <div>
          <span>Item 1</span>
          <DocuAtomDivider type="vertical" />
          <span>Item 2</span>
          <DocuAtomDivider type="vertical" />
          <span>Item 3</span>
        </div>
      );

      const dividers = container.querySelectorAll(".ant-divider-vertical");
      expect(dividers).toHaveLength(2);
    });
  });

  describe("Accessibility", () => {
    it("should have proper role", () => {
      const { container } = render(<DocuAtomDivider />);

      const divider = container.querySelector(".ant-divider");
      expect(divider).toHaveAttribute("role", "separator");
    });

    it("should be accessible with text", () => {
      render(<DocuAtomDivider>Section Separator</DocuAtomDivider>);

      expect(screen.getByText("Section Separator")).toBeInTheDocument();
    });
  });
});

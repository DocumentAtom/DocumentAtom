import { render, screen } from "@testing-library/react";
import DocuAtomCard from "#/components/base/card/Card";

describe("DocuAtomCard", () => {
  describe("Rendering", () => {
    it("should render card with children", () => {
      render(<DocuAtomCard>Card Content</DocuAtomCard>);

      expect(screen.getByText("Card Content")).toBeInTheDocument();
    });

    it("should render card with title", () => {
      render(<DocuAtomCard title="Card Title">Content</DocuAtomCard>);

      expect(screen.getByText("Card Title")).toBeInTheDocument();
      expect(screen.getByText("Content")).toBeInTheDocument();
    });

    it("should render card without title", () => {
      render(<DocuAtomCard>Content only</DocuAtomCard>);

      expect(screen.getByText("Content only")).toBeInTheDocument();
    });

    it("should render bordered card by default", () => {
      const { container } = render(<DocuAtomCard>Content</DocuAtomCard>);

      const card = container.querySelector(".ant-card");
      expect(card).toBeInTheDocument();
      expect(card).not.toHaveClass("ant-card-bordered-false");
    });
  });

  describe("Card sizes", () => {
    it("should render default size card", () => {
      const { container } = render(<DocuAtomCard>Content</DocuAtomCard>);

      const card = container.querySelector(".ant-card");
      expect(card).toBeInTheDocument();
    });

    it("should render small card", () => {
      const { container } = render(
        <DocuAtomCard size="small">Content</DocuAtomCard>
      );

      const card = container.querySelector(".ant-card-small");
      expect(card).toBeInTheDocument();
    });
  });

  describe("Card styling", () => {
    it("should apply custom className", () => {
      const { container } = render(
        <DocuAtomCard className="custom-card">Content</DocuAtomCard>
      );

      const card = container.querySelector(".custom-card");
      expect(card).toBeInTheDocument();
    });

    it("should apply custom style", () => {
      const { container } = render(
        <DocuAtomCard style={{ backgroundColor: "lightblue" }}>
          Content
        </DocuAtomCard>
      );

      const card = container.querySelector(".ant-card");
      expect(card).toHaveStyle({ backgroundColor: "lightblue" });
    });
  });

  describe("Card extras", () => {
    it("should render extra content", () => {
      render(
        <DocuAtomCard title="Title" extra={<button>Extra</button>}>
          Content
        </DocuAtomCard>
      );

      expect(screen.getByRole("button", { name: "Extra" })).toBeInTheDocument();
    });

    it("should render cover image", () => {
      render(
        <DocuAtomCard
          cover={<img alt="cover" src="/test.jpg" data-testid="cover-img" />}
        >
          Content
        </DocuAtomCard>
      );

      expect(screen.getByTestId("cover-img")).toBeInTheDocument();
    });

    it("should render actions", () => {
      const actions = [
        <button key="action1">Action 1</button>,
        <button key="action2">Action 2</button>,
      ];

      render(<DocuAtomCard actions={actions}>Content</DocuAtomCard>);

      expect(
        screen.getByRole("button", { name: "Action 1" })
      ).toBeInTheDocument();
      expect(
        screen.getByRole("button", { name: "Action 2" })
      ).toBeInTheDocument();
    });
  });

  describe("Card with loading state", () => {
    it("should render loading card", () => {
      const { container } = render(
        <DocuAtomCard loading>Content</DocuAtomCard>
      );

      const loadingCard = container.querySelector(".ant-card-loading");
      expect(loadingCard).toBeInTheDocument();
    });

    it("should not show content when loading", () => {
      const { container } = render(
        <DocuAtomCard loading>Content</DocuAtomCard>
      );

      // Skeleton loading should be shown instead
      const skeleton = container.querySelector(".ant-skeleton");
      expect(skeleton).toBeInTheDocument();
    });
  });

  describe("Card types", () => {
    it("should render with inner type", () => {
      const { container } = render(
        <DocuAtomCard type="inner">Content</DocuAtomCard>
      );

      const card = container.querySelector(".ant-card-type-inner");
      expect(card).toBeInTheDocument();
    });
  });

  describe("Hoverable card", () => {
    it("should render hoverable card", () => {
      const { container } = render(
        <DocuAtomCard hoverable>Content</DocuAtomCard>
      );

      const card = container.querySelector(".ant-card-hoverable");
      expect(card).toBeInTheDocument();
    });

    it("should render non-hoverable card by default", () => {
      const { container } = render(<DocuAtomCard>Content</DocuAtomCard>);

      const card = container.querySelector(".ant-card-hoverable");
      expect(card).not.toBeInTheDocument();
    });
  });

  describe("Complex content", () => {
    it("should render JSX children", () => {
      render(
        <DocuAtomCard>
          <div>
            <h1>Heading</h1>
            <p>Paragraph</p>
          </div>
        </DocuAtomCard>
      );

      expect(screen.getByText("Heading")).toBeInTheDocument();
      expect(screen.getByText("Paragraph")).toBeInTheDocument();
    });

    it("should render multiple children", () => {
      render(
        <DocuAtomCard>
          <p>First child</p>
          <p>Second child</p>
          <p>Third child</p>
        </DocuAtomCard>
      );

      expect(screen.getByText("First child")).toBeInTheDocument();
      expect(screen.getByText("Second child")).toBeInTheDocument();
      expect(screen.getByText("Third child")).toBeInTheDocument();
    });
  });

  describe("Edge Cases", () => {
    it("should handle empty children", () => {
      const { container } = render(<DocuAtomCard />);

      const card = container.querySelector(".ant-card");
      expect(card).toBeInTheDocument();
    });

    it("should handle very long content", () => {
      const longContent = "A".repeat(1000);
      render(<DocuAtomCard>{longContent}</DocuAtomCard>);

      expect(screen.getByText(longContent)).toBeInTheDocument();
    });

    it("should handle special characters", () => {
      render(<DocuAtomCard>!@#$%^&*()</DocuAtomCard>);

      expect(screen.getByText("!@#$%^&*()")).toBeInTheDocument();
    });

    it("should handle unicode characters", () => {
      render(<DocuAtomCard>卡片内容 🎴</DocuAtomCard>);

      expect(screen.getByText("卡片内容 🎴")).toBeInTheDocument();
    });

    it("should handle null children gracefully", () => {
      const { container } = render(<DocuAtomCard>{null}</DocuAtomCard>);

      const card = container.querySelector(".ant-card");
      expect(card).toBeInTheDocument();
    });
  });
});

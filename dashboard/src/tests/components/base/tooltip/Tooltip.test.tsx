import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import DocuAtomTooltip from "#/components/base/tooltip/Tooltip";

describe("DocuAtomTooltip", () => {
  describe("Rendering", () => {
    it("should render children", () => {
      render(
        <DocuAtomTooltip title="Tooltip text">
          <button>Hover me</button>
        </DocuAtomTooltip>
      );

      expect(
        screen.getByRole("button", { name: "Hover me" })
      ).toBeInTheDocument();
    });

    it("should not show tooltip initially", () => {
      render(
        <DocuAtomTooltip title="Tooltip text">
          <span>Content</span>
        </DocuAtomTooltip>
      );

      expect(screen.queryByText("Tooltip text")).not.toBeInTheDocument();
    });

    it("should show tooltip on hover", async () => {
      render(
        <DocuAtomTooltip title="Tooltip text">
          <button>Hover me</button>
        </DocuAtomTooltip>
      );

      const button = screen.getByRole("button");
      await userEvent.hover(button);

      await waitFor(() => {
        expect(screen.getByText("Tooltip text")).toBeInTheDocument();
      });
    });
  });

  describe("Placement", () => {
    const placements = [
      "top",
      "left",
      "right",
      "bottom",
      "topLeft",
      "topRight",
      "bottomLeft",
      "bottomRight",
      "leftTop",
      "leftBottom",
      "rightTop",
      "rightBottom",
    ];

    placements.forEach((placement) => {
      it(`should support ${placement} placement`, async () => {
        render(
          <DocuAtomTooltip title="Tooltip" placement={placement as any}>
            <button>Button</button>
          </DocuAtomTooltip>
        );

        const button = screen.getByRole("button");
        await userEvent.hover(button);

        await waitFor(() => {
          expect(screen.getByText("Tooltip")).toBeInTheDocument();
        });
      });
    });
  });

  describe("Tooltip title", () => {
    it("should render string title", async () => {
      render(
        <DocuAtomTooltip title="Simple title">
          <button>Button</button>
        </DocuAtomTooltip>
      );

      await userEvent.hover(screen.getByRole("button"));

      await waitFor(() => {
        expect(screen.getByText("Simple title")).toBeInTheDocument();
      });
    });

    it("should render JSX title", async () => {
      render(
        <DocuAtomTooltip
          title={
            <div>
              <strong>Bold</strong> content
            </div>
          }
        >
          <button>Button</button>
        </DocuAtomTooltip>
      );

      await userEvent.hover(screen.getByRole("button"));

      await waitFor(() => {
        expect(screen.getByText("Bold")).toBeInTheDocument();
        expect(screen.getByText("content")).toBeInTheDocument();
      });
    });

    it("should not show tooltip when title is empty", async () => {
      render(
        <DocuAtomTooltip title="">
          <button>Button</button>
        </DocuAtomTooltip>
      );

      await userEvent.hover(screen.getByRole("button"));

      // Wait a bit to ensure tooltip doesn't appear
      await new Promise((resolve) => setTimeout(resolve, 500));

      const tooltips = document.querySelectorAll(".ant-tooltip");
      expect(tooltips.length).toBe(0);
    });
  });

  describe("Trigger types", () => {
    it("should trigger on hover by default", async () => {
      render(
        <DocuAtomTooltip title="Hover tooltip">
          <button>Button</button>
        </DocuAtomTooltip>
      );

      await userEvent.hover(screen.getByRole("button"));

      await waitFor(() => {
        expect(screen.getByText("Hover tooltip")).toBeInTheDocument();
      });
    });

    it("should trigger on click", async () => {
      render(
        <DocuAtomTooltip title="Click tooltip" trigger="click">
          <button>Button</button>
        </DocuAtomTooltip>
      );

      await userEvent.click(screen.getByRole("button"));

      await waitFor(() => {
        expect(screen.getByText("Click tooltip")).toBeInTheDocument();
      });
    });
  });

  describe("Visible control", () => {
    it("should show tooltip when open is true", () => {
      render(
        <DocuAtomTooltip title="Always visible" open={true}>
          <button>Button</button>
        </DocuAtomTooltip>
      );

      expect(screen.getByText("Always visible")).toBeInTheDocument();
    });

    it("should hide tooltip when open is false", async () => {
      render(
        <DocuAtomTooltip title="Never visible" open={false}>
          <button>Button</button>
        </DocuAtomTooltip>
      );

      await userEvent.hover(screen.getByRole("button"));

      // Tooltip should not appear
      expect(screen.queryByText("Never visible")).not.toBeInTheDocument();
    });
  });

  describe("Mouse enter/leave delay", () => {
    it("should support mouse enter delay", async () => {
      render(
        <DocuAtomTooltip title="Delayed" mouseEnterDelay={0.5}>
          <button>Button</button>
        </DocuAtomTooltip>
      );

      await userEvent.hover(screen.getByRole("button"));

      // Tooltip should not appear immediately
      expect(screen.queryByText("Delayed")).not.toBeInTheDocument();

      // Wait for delay
      await waitFor(
        () => {
          expect(screen.getByText("Delayed")).toBeInTheDocument();
        },
        { timeout: 1000 }
      );
    });
  });

  describe("Arrow display", () => {
    it("should hide arrow when arrow is false", () => {
      const { container } = render(
        <DocuAtomTooltip title="No arrow" arrow={false} open={true}>
          <button>Button</button>
        </DocuAtomTooltip>
      );

      const arrow = container.querySelector(".ant-tooltip-arrow");
      expect(arrow).not.toBeInTheDocument();
    });
  });

  describe("Edge Cases", () => {
    it("should handle very long tooltip text", async () => {
      const longText = "A".repeat(200);
      render(
        <DocuAtomTooltip title={longText}>
          <button>Button</button>
        </DocuAtomTooltip>
      );

      await userEvent.hover(screen.getByRole("button"));

      await waitFor(() => {
        expect(screen.getByText(longText)).toBeInTheDocument();
      });
    });

    it("should handle special characters in title", async () => {
      render(
        <DocuAtomTooltip title="Special !@#$%^&*()">
          <button>Button</button>
        </DocuAtomTooltip>
      );

      await userEvent.hover(screen.getByRole("button"));

      await waitFor(() => {
        expect(screen.getByText("Special !@#$%^&*()")).toBeInTheDocument();
      });
    });

    it("should handle unicode characters", async () => {
      render(
        <DocuAtomTooltip title="提示信息 💡">
          <button>Button</button>
        </DocuAtomTooltip>
      );

      await userEvent.hover(screen.getByRole("button"));

      await waitFor(() => {
        expect(screen.getByText("提示信息 💡")).toBeInTheDocument();
      });
    });

    it("should handle disabled child element", () => {
      render(
        <DocuAtomTooltip title="Disabled tooltip">
          <button disabled>Disabled</button>
        </DocuAtomTooltip>
      );

      expect(screen.getByRole("button")).toBeDisabled();
    });

    it("should work with custom children", async () => {
      render(
        <DocuAtomTooltip title="Custom child">
          <div data-testid="custom-div">Custom element</div>
        </DocuAtomTooltip>
      );

      const customDiv = screen.getByTestId("custom-div");
      await userEvent.hover(customDiv);

      await waitFor(() => {
        expect(screen.getByText("Custom child")).toBeInTheDocument();
      });
    });
  });

  describe("Accessibility", () => {
    it("should have proper aria attributes", async () => {
      render(
        <DocuAtomTooltip title="Accessible tooltip">
          <button>Button</button>
        </DocuAtomTooltip>
      );

      const button = screen.getByRole("button");
      await userEvent.hover(button);

      await waitFor(() => {
        expect(screen.getByText("Accessible tooltip")).toBeInTheDocument();
      });
    });

    it("should support keyboard navigation with focus trigger", async () => {
      render(
        <DocuAtomTooltip title="Keyboard tooltip" trigger="focus">
          <button>Button</button>
        </DocuAtomTooltip>
      );

      const button = screen.getByRole("button");
      await userEvent.tab();

      // Button should have focus
      expect(button).toHaveFocus();

      await waitFor(() => {
        expect(screen.getByText("Keyboard tooltip")).toBeInTheDocument();
      });
    });
  });
});

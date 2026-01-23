import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import WeAlert from "#/components/base/alert/Alert";

describe("WeAlert", () => {
  describe("Rendering", () => {
    it("should render alert with message", () => {
      render(<WeAlert message="Alert message" />);

      expect(screen.getByText("Alert message")).toBeInTheDocument();
    });

    it("should render alert with description", () => {
      render(<WeAlert message="Title" description="Description text" />);

      expect(screen.getByText("Title")).toBeInTheDocument();
      expect(screen.getByText("Description text")).toBeInTheDocument();
    });

    it("should render info alert by default", () => {
      const { container } = render(<WeAlert message="Info" />);

      const alert = container.querySelector(".ant-alert-info");
      expect(alert).toBeInTheDocument();
    });
  });

  describe("Alert types", () => {
    it("should render success alert", () => {
      const { container } = render(
        <WeAlert type="success" message="Success" />
      );

      const alert = container.querySelector(".ant-alert-success");
      expect(alert).toBeInTheDocument();
    });

    it("should render error alert", () => {
      const { container } = render(<WeAlert type="error" message="Error" />);

      const alert = container.querySelector(".ant-alert-error");
      expect(alert).toBeInTheDocument();
    });

    it("should render warning alert", () => {
      const { container } = render(
        <WeAlert type="warning" message="Warning" />
      );

      const alert = container.querySelector(".ant-alert-warning");
      expect(alert).toBeInTheDocument();
    });

    it("should render info alert", () => {
      const { container } = render(<WeAlert type="info" message="Info" />);

      const alert = container.querySelector(".ant-alert-info");
      expect(alert).toBeInTheDocument();
    });
  });

  describe("Closable alert", () => {
    it("should render alert without close button by default", () => {
      render(<WeAlert message="Message" />);

      const closeButton = screen.queryByRole("button");
      expect(closeButton).not.toBeInTheDocument();
    });

    it("should render closable alert", () => {
      render(<WeAlert message="Message" closable />);

      const closeButton = screen.getByRole("button");
      expect(closeButton).toBeInTheDocument();
    });

    it("should call onClose when close button is clicked", async () => {
      const handleClose = jest.fn();
      render(<WeAlert message="Message" closable onClose={handleClose} />);

      const closeButton = screen.getByRole("button");
      await userEvent.click(closeButton);

      expect(handleClose).toHaveBeenCalledTimes(1);
    });

    it("should render with custom close text", () => {
      render(<WeAlert message="Message" closable closeText="Dismiss" />);

      expect(screen.getByText("Dismiss")).toBeInTheDocument();
    });
  });

  describe("Alert with icon", () => {
    it("should show icon when showIcon is true", () => {
      const { container } = render(<WeAlert message="Message" showIcon />);

      const icon = container.querySelector(".ant-alert-icon");
      expect(icon).toBeInTheDocument();
    });

    it("should hide icon when showIcon is false", () => {
      const { container } = render(
        <WeAlert message="Message" showIcon={false} />
      );

      const icon = container.querySelector(".ant-alert-icon");
      expect(icon).not.toBeInTheDocument();
    });
  });

  describe("Banner style", () => {
    it("should render regular alert by default", () => {
      const { container } = render(<WeAlert message="Message" />);

      const banner = container.querySelector(".ant-alert-banner");
      expect(banner).not.toBeInTheDocument();
    });

    it("should render banner style alert", () => {
      const { container } = render(<WeAlert message="Message" banner />);

      const banner = container.querySelector(".ant-alert-banner");
      expect(banner).toBeInTheDocument();
    });
  });

  describe("Alert with action", () => {
    it("should render action button", () => {
      const action = <button>Action</button>;
      render(<WeAlert message="Message" action={action} />);

      expect(
        screen.getByRole("button", { name: "Action" })
      ).toBeInTheDocument();
    });

    it("should handle action button click", async () => {
      const handleAction = jest.fn();
      const action = <button onClick={handleAction}>Action</button>;
      render(<WeAlert message="Message" action={action} />);

      const actionButton = screen.getByRole("button", { name: "Action" });
      await userEvent.click(actionButton);

      expect(handleAction).toHaveBeenCalledTimes(1);
    });
  });

  describe("Alert styling", () => {
    it("should apply custom className", () => {
      const { container } = render(
        <WeAlert message="Message" className="custom-alert" />
      );

      const alert = container.querySelector(".custom-alert");
      expect(alert).toBeInTheDocument();
    });

    it("should apply custom style", () => {
      const { container } = render(
        <WeAlert message="Message" style={{ backgroundColor: "lightblue" }} />
      );

      const alert = container.querySelector(".ant-alert");
      expect(alert).toHaveStyle({ backgroundColor: "lightblue" });
    });
  });

  describe("Complex content", () => {
    it("should render JSX message", () => {
      render(
        <WeAlert
          message={
            <div>
              <strong>Important:</strong> Message
            </div>
          }
        />
      );

      expect(screen.getByText("Important:")).toBeInTheDocument();
      expect(screen.getByText("Message")).toBeInTheDocument();
    });

    it("should render JSX description", () => {
      render(
        <WeAlert
          message="Title"
          description={
            <ul>
              <li>Item 1</li>
              <li>Item 2</li>
            </ul>
          }
        />
      );

      expect(screen.getByText("Item 1")).toBeInTheDocument();
      expect(screen.getByText("Item 2")).toBeInTheDocument();
    });
  });

  describe("Edge Cases", () => {
    it("should handle very long message", () => {
      const longMessage = "A".repeat(200);
      render(<WeAlert message={longMessage} />);

      expect(screen.getByText(longMessage)).toBeInTheDocument();
    });

    it("should handle very long description", () => {
      const longDesc = "B".repeat(500);
      render(<WeAlert message="Title" description={longDesc} />);

      expect(screen.getByText(longDesc)).toBeInTheDocument();
    });

    it("should handle special characters", () => {
      render(<WeAlert message="Alert: !@#$%^&*()" />);

      expect(screen.getByText("Alert: !@#$%^&*()")).toBeInTheDocument();
    });

    it("should handle unicode characters", () => {
      render(<WeAlert message="警告消息 ⚠️" />);

      expect(screen.getByText("警告消息 ⚠️")).toBeInTheDocument();
    });

    it("should handle empty message gracefully", () => {
      const { container } = render(<WeAlert message="" />);

      const alert = container.querySelector(".ant-alert");
      expect(alert).toBeInTheDocument();
    });
  });

  describe("Accessibility", () => {
    it("should have proper role", () => {
      const { container } = render(<WeAlert message="Message" />);

      const alert = container.querySelector(".ant-alert");
      expect(alert).toHaveAttribute("role", "alert");
    });

    it("should be keyboard accessible when closable", async () => {
      const handleClose = jest.fn();
      render(<WeAlert message="Message" closable onClose={handleClose} />);

      const closeButton = screen.getByRole("button");
      closeButton.focus();

      expect(closeButton).toHaveFocus();

      await userEvent.keyboard("{Enter}");

      expect(handleClose).toHaveBeenCalled();
    });
  });

  describe("Different type combinations", () => {
    it("should render success alert with description and icon", () => {
      const { container } = render(
        <WeAlert
          type="success"
          message="Success"
          description="Operation completed"
          showIcon
        />
      );

      expect(container.querySelector(".ant-alert-success")).toBeInTheDocument();
      expect(container.querySelector(".ant-alert-icon")).toBeInTheDocument();
      expect(screen.getByText("Success")).toBeInTheDocument();
      expect(screen.getByText("Operation completed")).toBeInTheDocument();
    });

    it("should render error alert with action and close", () => {
      const action = <button>Retry</button>;
      render(
        <WeAlert
          type="error"
          message="Error occurred"
          action={action}
          closable
        />
      );

      expect(screen.getByText("Error occurred")).toBeInTheDocument();
      expect(screen.getByRole("button", { name: "Retry" })).toBeInTheDocument();
      expect(screen.getAllByRole("button")).toHaveLength(2); // Retry + Close
    });
  });
});

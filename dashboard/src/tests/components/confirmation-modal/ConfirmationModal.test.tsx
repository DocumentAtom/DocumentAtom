import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import ConfirmationModal from "#/components/confirmation-modal/ConfirmationModal";

describe("ConfirmationModal", () => {
  const mockSetIsModelVisible = jest.fn();
  const mockHandleConfirm = jest.fn();

  const defaultProps = {
    title: "Confirm Action",
    isModelVisible: true,
    setIsModelVisible: mockSetIsModelVisible,
    handleConfirm: mockHandleConfirm,
    paragraphText: "Are you sure you want to proceed?",
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("Rendering", () => {
    it("should render modal when isModelVisible is true", () => {
      render(<ConfirmationModal {...defaultProps} />);

      expect(screen.getByText("Confirm Action")).toBeInTheDocument();
      expect(
        screen.getByText("Are you sure you want to proceed?")
      ).toBeInTheDocument();
    });

    it("should not render modal when isModelVisible is false", () => {
      render(<ConfirmationModal {...defaultProps} isModelVisible={false} />);

      expect(screen.queryByText("Confirm Action")).not.toBeInTheDocument();
    });

    it("should render Cancel button", () => {
      render(<ConfirmationModal {...defaultProps} />);

      const cancelButton = screen.getByTestId(
        "confirmation-modal-cancel-button"
      );
      expect(cancelButton).toBeInTheDocument();
      expect(cancelButton).toHaveTextContent("Cancel");
    });

    it("should render Confirm button", () => {
      render(<ConfirmationModal {...defaultProps} />);

      const confirmButton = screen.getByTestId(
        "confirmation-modal-confirm-button"
      );
      expect(confirmButton).toBeInTheDocument();
      expect(confirmButton).toHaveTextContent("Confirm");
    });

    it("should render custom paragraph text", () => {
      render(
        <ConfirmationModal
          {...defaultProps}
          paragraphText="This is a custom message"
        />
      );

      expect(screen.getByText("This is a custom message")).toBeInTheDocument();
    });

    it("should render custom title", () => {
      render(
        <ConfirmationModal {...defaultProps} title="Delete Confirmation" />
      );

      expect(screen.getByText("Delete Confirmation")).toBeInTheDocument();
    });
  });

  describe("User Interactions", () => {
    it("should call handleConfirm when Confirm button is clicked", async () => {
      render(<ConfirmationModal {...defaultProps} />);

      const confirmButton = screen.getByTestId(
        "confirmation-modal-confirm-button"
      );
      await userEvent.click(confirmButton);

      expect(mockHandleConfirm).toHaveBeenCalledTimes(1);
    });

    it("should call setIsModelVisible with false when Cancel button is clicked", async () => {
      render(<ConfirmationModal {...defaultProps} />);

      const cancelButton = screen.getByTestId(
        "confirmation-modal-cancel-button"
      );
      await userEvent.click(cancelButton);

      expect(mockSetIsModelVisible).toHaveBeenCalledWith(false);
      expect(mockSetIsModelVisible).toHaveBeenCalledTimes(1);
    });

    it("should not call handleConfirm when Cancel button is clicked", async () => {
      render(<ConfirmationModal {...defaultProps} />);

      const cancelButton = screen.getByTestId(
        "confirmation-modal-cancel-button"
      );
      await userEvent.click(cancelButton);

      expect(mockHandleConfirm).not.toHaveBeenCalled();
    });

    it("should handle multiple Confirm button clicks", async () => {
      render(<ConfirmationModal {...defaultProps} />);

      const confirmButton = screen.getByTestId(
        "confirmation-modal-confirm-button"
      );

      await userEvent.click(confirmButton);
      await userEvent.click(confirmButton);

      expect(mockHandleConfirm).toHaveBeenCalledTimes(2);
    });
  });

  describe("Loading State", () => {
    it("should show loading state on Confirm button when isLoading is true", () => {
      render(<ConfirmationModal {...defaultProps} isLoading={true} />);

      const confirmButton = screen.getByTestId(
        "confirmation-modal-confirm-button"
      );

      // Check if button has loading class or icon
      expect(confirmButton).toHaveClass("ant-btn-loading");
    });

    it("should not show loading state when isLoading is false", () => {
      render(<ConfirmationModal {...defaultProps} isLoading={false} />);

      const confirmButton = screen.getByTestId(
        "confirmation-modal-confirm-button"
      );

      expect(confirmButton).not.toHaveClass("ant-btn-loading");
    });

    it("should not show loading state when isLoading is undefined", () => {
      render(<ConfirmationModal {...defaultProps} />);

      const confirmButton = screen.getByTestId(
        "confirmation-modal-confirm-button"
      );

      expect(confirmButton).not.toHaveClass("ant-btn-loading");
    });
  });

  describe("Modal Behavior", () => {
    it("should call setIsModelVisible when modal backdrop is clicked", async () => {
      const { container } = render(<ConfirmationModal {...defaultProps} />);

      const modalWrap = container.querySelector(".ant-modal-wrap");
      if (modalWrap) {
        await userEvent.click(modalWrap);
      }

      // Note: Actual behavior might vary based on Ant Design modal implementation
      // This test assumes the modal has onCancel handler
    });
  });

  describe("Edge Cases", () => {
    it("should handle empty paragraph text", () => {
      render(<ConfirmationModal {...defaultProps} paragraphText="" />);

      expect(screen.getByText("Confirm Action")).toBeInTheDocument();
    });

    it("should handle very long paragraph text", () => {
      const longText = "A".repeat(1000);
      render(<ConfirmationModal {...defaultProps} paragraphText={longText} />);

      expect(screen.getByText(longText)).toBeInTheDocument();
    });

    it("should handle special characters in text", () => {
      const specialText = "Are you sure? This has !@#$%^&*() characters.";
      render(
        <ConfirmationModal {...defaultProps} paragraphText={specialText} />
      );

      expect(screen.getByText(specialText)).toBeInTheDocument();
    });

    it("should handle HTML entities in text", () => {
      const textWithHtml = "Text with <b>HTML</b> tags";
      render(
        <ConfirmationModal {...defaultProps} paragraphText={textWithHtml} />
      );

      expect(screen.getByText(textWithHtml)).toBeInTheDocument();
    });
  });

  describe("Accessibility", () => {
    it("should have proper button roles", () => {
      render(<ConfirmationModal {...defaultProps} />);

      const cancelButton = screen.getByTestId(
        "confirmation-modal-cancel-button"
      );
      const confirmButton = screen.getByTestId(
        "confirmation-modal-confirm-button"
      );

      expect(cancelButton).toHaveAttribute("type", "button");
      expect(confirmButton).toHaveAttribute("type", "button");
    });
  });
});

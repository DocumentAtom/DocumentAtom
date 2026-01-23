import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import DocuAtomModal from "#/components/base/modal/Modal";

describe("DocuAtomModal", () => {
  it("should render modal when open", () => {
    render(
      <DocuAtomModal open title="Test Modal">
        Modal Content
      </DocuAtomModal>
    );

    expect(screen.getByText("Test Modal")).toBeInTheDocument();
    expect(screen.getByText("Modal Content")).toBeInTheDocument();
  });

  it("should not render when closed", () => {
    render(
      <DocuAtomModal open={false} title="Test Modal">
        Modal Content
      </DocuAtomModal>
    );

    expect(screen.queryByText("Test Modal")).not.toBeInTheDocument();
  });

  it("should call onCancel when cancelled", async () => {
    const handleCancel = jest.fn();
    render(
      <DocuAtomModal open onCancel={handleCancel} title="Test">
        Content
      </DocuAtomModal>
    );

    const cancelButton = screen.getByRole("button", { name: /cancel/i });
    await userEvent.click(cancelButton);

    expect(handleCancel).toHaveBeenCalled();
  });

  it("should render footer", () => {
    render(
      <DocuAtomModal open footer={<button>Custom Footer</button>}>
        Content
      </DocuAtomModal>
    );

    expect(
      screen.getByRole("button", { name: "Custom Footer" })
    ).toBeInTheDocument();
  });

  it("should match snapshot", () => {
    const { container } = render(
      <DocuAtomModal open title="Test">
        Content
      </DocuAtomModal>
    );

    expect(container.firstChild).toMatchSnapshot();
  });
});

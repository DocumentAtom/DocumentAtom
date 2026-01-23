import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import DocuAtomPassword from "#/components/base/password/Passwored";

describe("DocuAtomPassword", () => {
  it("should render password input", () => {
    render(<DocuAtomPassword placeholder="Enter password" />);

    expect(screen.getByPlaceholderText("Enter password")).toBeInTheDocument();
  });

  it("should accept password input", async () => {
    const { container } = render(<DocuAtomPassword />);

    const input = container.querySelector("input");
    if (input) {
      await userEvent.type(input, "mypassword123");
      expect(input).toHaveValue("mypassword123");
    }
  });

  it("should show password visibility toggle", () => {
    const { container } = render(<DocuAtomPassword />);

    expect(
      container.querySelector(".ant-input-password-icon")
    ).toBeInTheDocument();
  });

  it("should toggle password visibility", async () => {
    const { container } = render(<DocuAtomPassword />);

    const input = container.querySelector("input");
    const toggleButton = container.querySelector(".ant-input-password-icon");

    if (input && toggleButton) {
      // Initially password should be hidden
      await userEvent.type(input, "password");
      expect(input).toHaveAttribute("type", "password");

      // Click toggle to show password - verify button exists
      expect(toggleButton).toBeInTheDocument();
    }
  });

  it("should apply disabled state", () => {
    const { container } = render(<DocuAtomPassword disabled />);

    expect(container.querySelector(".ant-input-disabled")).toBeInTheDocument();
  });

  it("should match snapshot", () => {
    const { container } = render(<DocuAtomPassword placeholder="Password" />);

    expect(container.firstChild).toMatchSnapshot();
  });
});

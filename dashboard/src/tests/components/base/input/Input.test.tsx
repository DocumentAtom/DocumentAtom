import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import DocuAtomInput from "#/components/base/input/Input";

describe("DocuAtomInput", () => {
  it("should render input", () => {
    render(<DocuAtomInput placeholder="Enter text" />);

    expect(screen.getByPlaceholderText("Enter text")).toBeInTheDocument();
  });

  it("should accept user input", async () => {
    render(<DocuAtomInput />);

    const input = screen.getByRole("textbox");
    await userEvent.type(input, "test value");

    expect(input).toHaveValue("test value");
  });

  it("should apply disabled state", () => {
    render(<DocuAtomInput disabled />);

    expect(screen.getByRole("textbox")).toBeDisabled();
  });

  it("should call onChange", async () => {
    const handleChange = jest.fn();
    render(<DocuAtomInput onChange={handleChange} />);

    const input = screen.getByRole("textbox");
    await userEvent.type(input, "a");

    expect(handleChange).toHaveBeenCalled();
  });

  it("should match snapshot", () => {
    const { container } = render(<DocuAtomInput placeholder="Test" />);

    expect(container.firstChild).toMatchSnapshot();
  });
});

import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import DocuAtomSelect from "#/components/base/select/Select";

describe("DocuAtomSelect", () => {
  const options = [
    { value: "1", label: "Option 1" },
    { value: "2", label: "Option 2" },
    { value: "3", label: "Option 3" },
  ];

  it("should render select with placeholder", () => {
    render(<DocuAtomSelect options={options} placeholder="Select option" />);

    expect(screen.getByText("Select option")).toBeInTheDocument();
  });

  it("should render all options when clicked", async () => {
    render(<DocuAtomSelect options={options} />);

    const select = screen.getByRole("combobox");
    await userEvent.click(select);

    expect(screen.getByText("Option 1")).toBeInTheDocument();
    expect(screen.getByText("Option 2")).toBeInTheDocument();
    expect(screen.getByText("Option 3")).toBeInTheDocument();
  });

  it("should call onChange when option selected", async () => {
    const handleChange = jest.fn();
    render(<DocuAtomSelect options={options} onChange={handleChange} />);

    const select = screen.getByRole("combobox");
    await userEvent.click(select);
    await userEvent.click(screen.getByText("Option 1"));

    expect(handleChange).toHaveBeenCalled();
  });

  it("should apply disabled state", () => {
    const { container } = render(<DocuAtomSelect options={options} disabled />);

    expect(container.querySelector(".ant-select-disabled")).toBeInTheDocument();
  });

  it("should match snapshot", () => {
    const { container } = render(<DocuAtomSelect options={options} />);

    expect(container.firstChild).toMatchSnapshot();
  });
});

import { render, screen } from "@testing-library/react";
import DocuAtomFormItem from "#/components/base/form/FormItem";

describe("DocuAtomFormItem", () => {
  it("should render form item with label", () => {
    render(<DocuAtomFormItem label="Username">Input Field</DocuAtomFormItem>);

    expect(screen.getByText("Username")).toBeInTheDocument();
    expect(screen.getByText("Input Field")).toBeInTheDocument();
  });

  it("should render form item without label", () => {
    render(<DocuAtomFormItem>Input Field</DocuAtomFormItem>);

    expect(screen.getByText("Input Field")).toBeInTheDocument();
  });

  it("should render required field", () => {
    render(
      <DocuAtomFormItem label="Email" required>
        Input Field
      </DocuAtomFormItem>
    );

    expect(screen.getByText("Email")).toBeInTheDocument();
  });

  it("should apply custom className", () => {
    const { container } = render(
      <DocuAtomFormItem className="custom-form-item">Content</DocuAtomFormItem>
    );

    expect(container.querySelector(".custom-form-item")).toBeInTheDocument();
  });

  it("should apply validation rules", () => {
    render(
      <DocuAtomFormItem name="password" rules={[{ required: true }]}>
        Input
      </DocuAtomFormItem>
    );

    expect(screen.getByText("Input")).toBeInTheDocument();
  });

  it("should match snapshot", () => {
    const { container } = render(<DocuAtomFormItem>Content</DocuAtomFormItem>);

    expect(container.firstChild).toMatchSnapshot();
  });
});

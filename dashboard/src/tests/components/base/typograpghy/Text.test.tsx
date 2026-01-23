import { render, screen } from "@testing-library/react";
import DocuAtomText from "#/components/base/typograpghy/Text";

describe("DocuAtomText", () => {
  it("should render text", () => {
    render(<DocuAtomText>Sample text</DocuAtomText>);

    expect(screen.getByText("Sample text")).toBeInTheDocument();
  });

  it("should apply font weight", () => {
    const { container } = render(
      <DocuAtomText weight={700}>Bold text</DocuAtomText>
    );

    const text = container.querySelector(".ant-typography");
    expect(text).toHaveStyle({ fontWeight: 700 });
  });

  it("should apply font size", () => {
    const { container } = render(
      <DocuAtomText fontSize={20}>Large text</DocuAtomText>
    );

    const text = container.querySelector(".ant-typography");
    expect(text).toHaveStyle({ fontSize: "20px" });
  });

  it("should apply color", () => {
    const { container } = render(
      <DocuAtomText color="red">Colored text</DocuAtomText>
    );

    const text = container.querySelector(".ant-typography");
    expect(text).toHaveStyle({ color: "red" });
  });

  it("should render strong text", () => {
    render(<DocuAtomText strong>Strong text</DocuAtomText>);

    expect(screen.getByText("Strong text")).toBeInTheDocument();
  });

  it("should match snapshot", () => {
    const { container } = render(<DocuAtomText>Text</DocuAtomText>);

    expect(container.firstChild).toMatchSnapshot();
  });
});

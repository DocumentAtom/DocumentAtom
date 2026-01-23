import { render, screen } from "@testing-library/react";
import DocuAtomParagraph from "#/components/base/typograpghy/Paragraph";

describe("DocuAtomParagraph", () => {
  it("should render paragraph", () => {
    render(<DocuAtomParagraph>Paragraph text</DocuAtomParagraph>);

    expect(screen.getByText("Paragraph text")).toBeInTheDocument();
  });

  it("should apply custom font weight", () => {
    const { container } = render(
      <DocuAtomParagraph weight={600}>Bold paragraph</DocuAtomParagraph>
    );

    const paragraph = container.querySelector(".ant-typography");
    expect(paragraph).toHaveStyle({ fontWeight: 600 });
  });

  it("should apply font size", () => {
    const { container } = render(
      <DocuAtomParagraph fontSize={18}>Large paragraph</DocuAtomParagraph>
    );

    const paragraph = container.querySelector(".ant-typography");
    expect(paragraph).toHaveStyle({ fontSize: "18px" });
  });

  it("should apply color", () => {
    const { container } = render(
      <DocuAtomParagraph color="blue">Colored paragraph</DocuAtomParagraph>
    );

    const paragraph = container.querySelector(".ant-typography");
    expect(paragraph).toHaveStyle({ color: "blue" });
  });

  it("should match snapshot", () => {
    const { container } = render(<DocuAtomParagraph>Text</DocuAtomParagraph>);

    expect(container.firstChild).toMatchSnapshot();
  });
});

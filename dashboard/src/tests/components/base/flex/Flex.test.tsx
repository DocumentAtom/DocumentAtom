import { render, screen } from "@testing-library/react";
import DocuAtomFlex from "#/components/base/flex/Flex";

describe("DocuAtomFlex", () => {
  it("should render children", () => {
    render(<DocuAtomFlex>Content</DocuAtomFlex>);

    expect(screen.getByText("Content")).toBeInTheDocument();
  });

  it("should apply vertical direction", () => {
    const { container } = render(<DocuAtomFlex vertical>Content</DocuAtomFlex>);

    expect(container.querySelector(".ant-flex-vertical")).toBeInTheDocument();
  });

  it("should apply gap", () => {
    const { container } = render(<DocuAtomFlex gap={10}>Content</DocuAtomFlex>);

    const flex = container.querySelector(".ant-flex");
    expect(flex).toHaveStyle({ gap: "10px" });
  });

  it("should apply justify content", () => {
    render(<DocuAtomFlex justify="center">Content</DocuAtomFlex>);

    expect(screen.getByText("Content")).toBeInTheDocument();
  });

  it("should match snapshot", () => {
    const { container } = render(<DocuAtomFlex>Content</DocuAtomFlex>);

    expect(container.firstChild).toMatchSnapshot();
  });
});

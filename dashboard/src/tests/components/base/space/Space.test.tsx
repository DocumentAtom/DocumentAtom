import { render, screen } from "@testing-library/react";
import DocuAtomSpace from "#/components/base/space/Space";

describe("DocuAtomSpace", () => {
  it("should render children", () => {
    render(
      <DocuAtomSpace>
        <div>Item 1</div>
        <div>Item 2</div>
      </DocuAtomSpace>
    );

    expect(screen.getByText("Item 1")).toBeInTheDocument();
    expect(screen.getByText("Item 2")).toBeInTheDocument();
  });

  it("should apply size", () => {
    const { container } = render(
      <DocuAtomSpace size="large">
        <div>Item</div>
      </DocuAtomSpace>
    );

    expect(container.querySelector(".ant-space")).toBeInTheDocument();
  });

  it("should apply vertical direction", () => {
    const { container } = render(
      <DocuAtomSpace direction="vertical">
        <div>Item</div>
      </DocuAtomSpace>
    );

    expect(container.querySelector(".ant-space-vertical")).toBeInTheDocument();
  });

  it("should match snapshot", () => {
    const { container } = render(
      <DocuAtomSpace>
        <div>Item</div>
      </DocuAtomSpace>
    );

    expect(container.firstChild).toMatchSnapshot();
  });
});

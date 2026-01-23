import { render, screen } from "@testing-library/react";
import DocuAtomTitle from "#/components/base/typograpghy/Title";

describe("DocuAtomTitle", () => {
  it("should render title", () => {
    render(<DocuAtomTitle>Title Text</DocuAtomTitle>);

    expect(screen.getByText("Title Text")).toBeInTheDocument();
  });

  it("should render different levels", () => {
    render(<DocuAtomTitle level={3}>Level 3</DocuAtomTitle>);

    const heading = screen.getByRole("heading", { level: 3 });
    expect(heading).toBeInTheDocument();
  });

  it("should apply center alignment", () => {
    const { container } = render(
      <DocuAtomTitle center>Centered</DocuAtomTitle>
    );

    const title = container.querySelector(".ant-typography");
    expect(title).toHaveStyle({ textAlign: "center" });
  });

  it("should apply custom font weight", () => {
    const { container } = render(
      <DocuAtomTitle weight={800}>Bold</DocuAtomTitle>
    );

    const title = container.querySelector(".ant-typography");
    expect(title).toHaveStyle({ fontWeight: 800 });
  });

  it("should match snapshot", () => {
    const { container } = render(<DocuAtomTitle>Title</DocuAtomTitle>);

    expect(container.firstChild).toMatchSnapshot();
  });
});

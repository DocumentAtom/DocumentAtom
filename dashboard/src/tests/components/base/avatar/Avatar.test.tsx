import { render, screen } from "@testing-library/react";
import DocuAtomAvatar from "#/components/base/avatar/Avatar";

describe("DocuAtomAvatar", () => {
  it("should render avatar with children", () => {
    render(<DocuAtomAvatar>U</DocuAtomAvatar>);

    expect(screen.getByText("U")).toBeInTheDocument();
  });

  it("should render avatar with image", () => {
    render(<DocuAtomAvatar src="/avatar.png" alt="User Avatar" />);

    expect(screen.getByRole("img")).toBeInTheDocument();
  });

  it("should render different sizes", () => {
    const { container } = render(
      <DocuAtomAvatar size="large">A</DocuAtomAvatar>
    );

    expect(container.querySelector(".ant-avatar-lg")).toBeInTheDocument();
  });

  it("should apply custom style", () => {
    const { container } = render(
      <DocuAtomAvatar style={{ backgroundColor: "red" }}>A</DocuAtomAvatar>
    );

    const avatar = container.querySelector(".ant-avatar");
    expect(avatar).toHaveStyle({ backgroundColor: "red" });
  });

  it("should match snapshot", () => {
    const { container } = render(<DocuAtomAvatar>A</DocuAtomAvatar>);

    expect(container.firstChild).toMatchSnapshot();
  });
});

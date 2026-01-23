import { render, screen } from "@testing-library/react";
import Loading from "#/app/loading";

describe("Loading Component", () => {
  it("should render PageLoading component", () => {
    const { container } = render(<Loading />);

    expect(container.querySelector(".pageLoading")).toBeInTheDocument();
  });

  it("should display loading spinner", () => {
    render(<Loading />);

    expect(screen.getByRole("img", { hidden: true })).toBeInTheDocument();
  });

  it("should match snapshot", () => {
    const { container } = render(<Loading />);

    expect(container.firstChild).toMatchSnapshot();
  });
});

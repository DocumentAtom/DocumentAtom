import { screen } from "@testing-library/react";
import Home from "#/app/page";
import { renderWithRedux } from "../store/utils";

// Mock next/navigation
jest.mock("next/navigation", () => ({
  useRouter: () => ({
    push: jest.fn(),
    replace: jest.fn(),
    refresh: jest.fn(),
  }),
  usePathname: () => "/",
}));

describe("Home Page", () => {
  it("should render LoginPage component", () => {
    renderWithRedux(<Home />, true);

    expect(
      screen.getByLabelText(/DocumentAtom Server URL/i)
    ).toBeInTheDocument();
  });

  it("should render server URL input", () => {
    renderWithRedux(<Home />, true);

    expect(
      screen.getByPlaceholderText(/https:\/\/your-documentatom-server.com/i)
    ).toBeInTheDocument();
  });

  it("should match snapshot", () => {
    const { container } = renderWithRedux(<Home />, true);

    expect(container.firstChild).toMatchSnapshot();
  });
});

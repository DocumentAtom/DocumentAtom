import { screen, waitFor } from "@testing-library/react";
import DashboardPage from "#/app/dashboard/page";
import { renderWithRedux } from "../../store/utils";

// Mock next/navigation
jest.mock("next/navigation", () => ({
  useRouter: () => ({
    push: jest.fn(),
    replace: jest.fn(),
    refresh: jest.fn(),
  }),
  usePathname: () => "/dashboard",
}));

describe("Dashboard Page", () => {
  it("should render HomePage component", () => {
    renderWithRedux(<DashboardPage />);

    expect(screen.getByText("Home")).toBeInTheDocument();
  });

  it("should render tabs", () => {
    renderWithRedux(<DashboardPage />);

    expect(
      screen.getByRole("tab", { name: "Type Detection" })
    ).toBeInTheDocument();
    expect(
      screen.getByRole("tab", { name: "Atom Extraction" })
    ).toBeInTheDocument();
  });

  it("should render Type Detection tab by default", async () => {
    renderWithRedux(<DashboardPage />);

    await waitFor(() => {
      expect(
        screen.getByText("Type Detection", { selector: "h4" })
      ).toBeInTheDocument();
    });
  });

  it("should match snapshot", () => {
    const { container } = renderWithRedux(<DashboardPage />);

    expect(container.firstChild).toMatchSnapshot();
  });
});

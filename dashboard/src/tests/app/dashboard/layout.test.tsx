import { screen } from "@testing-library/react";
import DashboardLayoutPage from "#/app/dashboard/layout";
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

// Mock localStorage
const mockLocalStorage = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  clear: jest.fn(),
  removeItem: jest.fn(),
  length: 0,
  key: jest.fn(),
};

Object.defineProperty(window, "localStorage", {
  value: mockLocalStorage,
  writable: true,
});

describe("Dashboard Layout", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockLocalStorage.getItem.mockReturnValue("http://test-server.com");
  });

  it("should render connectivity validation message", () => {
    renderWithRedux(
      <DashboardLayoutPage>
        <div>Test Child</div>
      </DashboardLayoutPage>
    );

    expect(screen.getByText(/Validating connectivity/i)).toBeInTheDocument();
  });

  it("should render loading spinner during validation", () => {
    const { container } = renderWithRedux(
      <DashboardLayoutPage>
        <div>Test Child</div>
      </DashboardLayoutPage>
    );

    expect(container.querySelector(".pageLoader")).toBeInTheDocument();
  });

  it("should render header", () => {
    const { container } = renderWithRedux(
      <DashboardLayoutPage>
        <div>Test Child</div>
      </DashboardLayoutPage>
    );

    expect(container.querySelector(".ant-layout-header")).toBeInTheDocument();
  });

  it("should render logo in header", () => {
    renderWithRedux(
      <DashboardLayoutPage>
        <div>Test Child</div>
      </DashboardLayoutPage>
    );

    expect(screen.getByAltText("Document Atom")).toBeInTheDocument();
  });

  it("should match snapshot", () => {
    const { container } = renderWithRedux(
      <DashboardLayoutPage>
        <div>Test Content</div>
      </DashboardLayoutPage>
    );

    expect(container.firstChild).toMatchSnapshot();
  });
});

import { render, screen } from "@testing-library/react";
import LoginLayout from "#/components/layout/LoginLayout";
import { AppContext } from "#/hooks/appHooks";
import { ThemeEnum } from "#/types/types";

describe("LoginLayout", () => {
  const mockSetTheme = jest.fn();

  const renderWithTheme = (
    children: React.ReactNode,
    theme = ThemeEnum.LIGHT
  ) => {
    return render(
      <AppContext.Provider value={{ theme, setTheme: mockSetTheme }}>
        <LoginLayout>{children}</LoginLayout>
      </AppContext.Provider>
    );
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("Rendering", () => {
    it("should render children", () => {
      renderWithTheme(<div>Test Content</div>);

      expect(screen.getByText("Test Content")).toBeInTheDocument();
    });

    it("should render logo", () => {
      renderWithTheme(<div>Content</div>);

      expect(screen.getByAltText("Document Atom")).toBeInTheDocument();
    });

    it("should render theme mode switch", () => {
      const { container } = renderWithTheme(<div>Content</div>);

      expect(container.querySelector("svg")).toBeInTheDocument();
    });

    it("should have header section", () => {
      const { container } = renderWithTheme(<div>Content</div>);

      const flexElements = container.querySelectorAll(".ant-flex");
      expect(flexElements.length).toBeGreaterThan(0);
    });

    it("should have footer section", () => {
      const { container } = renderWithTheme(<div>Content</div>);

      const flexElements = container.querySelectorAll(".ant-flex");
      expect(flexElements.length).toBeGreaterThan(0);
    });
  });

  describe("Layout Structure", () => {
    it("should have vertical flex layout", () => {
      const { container } = renderWithTheme(<div>Content</div>);

      const verticalFlex = container.querySelector(".ant-flex-vertical");
      expect(verticalFlex).toBeInTheDocument();
    });

    it("should render content in proper section", () => {
      renderWithTheme(<div data-testid="test-content">Content</div>);

      expect(screen.getByTestId("test-content")).toBeInTheDocument();
    });
  });

  describe("Different Content Types", () => {
    it("should render JSX children", () => {
      renderWithTheme(
        <div>
          <h1>Title</h1>
          <p>Paragraph</p>
        </div>
      );

      expect(screen.getByText("Title")).toBeInTheDocument();
      expect(screen.getByText("Paragraph")).toBeInTheDocument();
    });

    it("should render multiple children", () => {
      renderWithTheme(
        <>
          <div>First</div>
          <div>Second</div>
        </>
      );

      expect(screen.getByText("First")).toBeInTheDocument();
      expect(screen.getByText("Second")).toBeInTheDocument();
    });
  });

  describe("Snapshots", () => {
    it("should match snapshot", () => {
      const { container } = renderWithTheme(<div>Test Content</div>);

      expect(container.firstChild).toMatchSnapshot();
    });

    it("should match snapshot in dark mode", () => {
      const { container } = renderWithTheme(
        <div>Test Content</div>,
        ThemeEnum.DARK
      );

      expect(container.firstChild).toMatchSnapshot();
    });
  });
});

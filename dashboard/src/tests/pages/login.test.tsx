import { screen, waitFor } from "@testing-library/react";
import LoginPage from "#/page/login/LoginPage";
import { renderWithRedux } from "../store/utils";
import { getServer, mockServerURL } from "#/tests/mocks/server";
import { successHandlers, errorHandlers } from "#/tests/mocks/handlers";
import { localStorageKeys } from "#/constants/constant";

// Get the mocked router from jest.setup.js
const mockPush = jest.fn();
const mockReplace = jest.fn();
const mockPrefetch = jest.fn();

// Mock next/navigation in this file
jest.mock("next/navigation", () => ({
  useRouter: () => ({
    push: mockPush,
    replace: mockReplace,
    prefetch: mockPrefetch,
  }),
  usePathname: () => "",
}));

describe("LoginPage", () => {
  let server: ReturnType<typeof getServer>;

  beforeEach(() => {
    jest.clearAllMocks();
    localStorage.clear();
    mockPush.mockClear();
    mockReplace.mockClear();
    mockPrefetch.mockClear();
  });

  afterEach(() => {
    if (server) {
      server.close();
    }
  });

  describe("Rendering", () => {
    it("should render login page with all elements", () => {
      server = getServer(successHandlers);
      server.listen();

      renderWithRedux(<LoginPage />);

      expect(
        screen.getByLabelText(/DocumentAtom Server URL/i)
      ).toBeInTheDocument();
      expect(
        screen.getByPlaceholderText(/https:\/\/your-documentatom-server.com/i)
      ).toBeInTheDocument();
      expect(screen.getByRole("button")).toBeInTheDocument();
    });
  });

  describe("LocalStorage Integration", () => {
    it("should load saved URL from localStorage on mount", async () => {
      const savedURL = "http://saved-url.com";
      localStorage.setItem(localStorageKeys.documentAtomAPIUrl, savedURL);

      server = getServer(successHandlers);
      server.listen();

      renderWithRedux(<LoginPage />);

      await waitFor(() => {
        const input = screen.getByLabelText(
          /DocumentAtom Server URL/i
        ) as HTMLInputElement;
        expect(input.value).toBe(savedURL);
      });
    });
  });

  describe("Auto-validation on Mount", () => {
    it("should auto-validate connectivity if URL exists in localStorage", async () => {
      const savedURL = mockServerURL;
      localStorage.setItem(localStorageKeys.documentAtomAPIUrl, savedURL);

      server = getServer(successHandlers);
      server.listen();

      renderWithRedux(<LoginPage />);

      // Should automatically validate and show success message
      await waitFor(
        () => {
          expect(
            screen.getByText(/Your DocumentAtom node is operational/i)
          ).toBeInTheDocument();
        },
        { timeout: 5000 }
      );
    });

    it("should show error if auto-validation fails", async () => {
      const savedURL = mockServerURL;
      localStorage.setItem(localStorageKeys.documentAtomAPIUrl, savedURL);

      server = getServer(errorHandlers);
      server.listen();

      renderWithRedux(<LoginPage />);

      // Should automatically validate and show error
      await waitFor(
        () => {
          expect(
            screen.getByText(/Unable to connect to DocumentAtom services/i)
          ).toBeInTheDocument();
        },
        { timeout: 5000 }
      );
    });
  });

  describe("UI States", () => {
    it("should show success status after auto-validation", async () => {
      const savedURL = mockServerURL;
      localStorage.setItem(localStorageKeys.documentAtomAPIUrl, savedURL);

      server = getServer(successHandlers);
      server.listen();

      renderWithRedux(<LoginPage />);

      await waitFor(
        () => {
          const successText = screen.getByText(
            /Your DocumentAtom node is operational/i
          );
          expect(successText).toBeInTheDocument();
          expect(successText).toHaveClass("text-color-success");
        },
        { timeout: 5000 }
      );
    });
  });
});

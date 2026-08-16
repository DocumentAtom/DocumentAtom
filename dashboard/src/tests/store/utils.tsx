import DashboardLayout from "#/components/layout/DashboardLayout";
import LoginLayout from "#/components/layout/LoginLayout";
import resettableRootReducer, { apiMiddleWares } from "#/store/rootReducer";
import { RootState } from "#/store/store";
import { configureStore } from "@reduxjs/toolkit";
import { App, ConfigProvider } from "antd";
import { render } from "@testing-library/react";

import { Provider } from "react-redux";
import { primaryTheme } from "#/theme/theme";

type TestLayout = "dashboard" | "login" | "none";

export const renderWithRedux = (
  ui: React.ReactNode,
  loginLayout?: boolean | TestLayout,
  reduxState?: RootState
) => {
  const reduxStore = reduxState
    ? configureStore({
        reducer: resettableRootReducer,
        preloadedState: reduxState as RootState,
        middleware: (gDM: any) =>
          gDM({
            serializableCheck: false,
          }).concat(apiMiddleWares),
      })
    : configureStore({
        reducer: resettableRootReducer,
        middleware: (gDM: any) =>
          gDM({
            serializableCheck: false,
          }).concat(apiMiddleWares),
      });
  const layout: TestLayout =
    loginLayout === true
      ? "login"
      : typeof loginLayout === "string"
        ? loginLayout
        : "dashboard";
  const content =
    layout === "login" ? (
      <LoginLayout>{ui}</LoginLayout>
    ) : layout === "dashboard" ? (
      <DashboardLayout>{ui}</DashboardLayout>
    ) : (
      ui
    );

  return render(
    <Provider store={reduxStore}>
      <ConfigProvider theme={primaryTheme}>
        <App message={{ maxCount: 1 }}>{content}</App>
      </ConfigProvider>
    </Provider>
  );
};

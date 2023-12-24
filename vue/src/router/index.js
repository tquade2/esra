import { createRouter as createRouter, createWebHistory } from "vue-router";
import { useStore } from "vuex";

import HomeView from "../views/HomeView.vue";
import LoginView from "../views/LoginView.vue";
import LogoutView from "../views/LogoutView.vue";
import RegisterView from "../views/RegisterView.vue";
import CallerView from "../views/CallerView.vue";
import ServiceRequestView from "../views/ServiceRequestView.vue";

const routes = [
  {
    path: "/",
    name: "home",
    component: HomeView,
    meta: { requiresAuth: true, },
  },
  {
    path: "/login",
    name: "login",
    component: LoginView,
    meta: { requiresAuth: false, },
  },
  {
    path: "/logout",
    name: "logout",
    component: LogoutView,
    meta: { requiresAuth: false, },
  },
  {
    path: "/register",
    name: "register",
    component: RegisterView,
    meta: { requiresAuth: false, },
  },
  {
    path: "/caller",
    name: "caller",
    component: CallerView,
    meta: { requiresAuth: true, },
  },
  {
    path: "/service-request",
    name: "serviceRequest",
    component: ServiceRequestView,
    meta: { requiresAuth: true, },
  },
];

// Create the router
const router = createRouter({
  history: createWebHistory(),
  routes: routes,
});

router.beforeEach((to) => {
  // Get the Vuex store
  const store = useStore();

  // Determine if the route requires Authentication
  const requiresAuth = to.matched.some((x) => x.meta.requiresAuth);

  // If it does and they are not logged in, send the user to "/login"
  if (requiresAuth && store.state.token === "") {
    return { name: "login" };
  }
  // Otherwise, do nothing and they'll go to their next destination
});

export default router;

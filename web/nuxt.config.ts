import tailwindcss from "@tailwindcss/vite";

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: "2025-07-15",
  devtools: { enabled: true },

  runtimeConfig: {
    public: {
      WS_URL: "",
      API_BASEURL: "",
    },
  },

  modules: ["@nuxt/eslint", "@nuxt/fonts", "@nuxt/hints", "shadcn-nuxt"],

  css: ["~/assets/css/tailwind.css"],
  vite: {
    plugins: [tailwindcss()],
  },

  shadcn: {
    /**
     * Prefix for all the imported component.
     * @default "Ui"
     */
    prefix: "",
    /**
     * Directory that the component lives in.
     * Will respect the Nuxt aliases.
     * @link https://nuxt.com/docs/api/nuxt-config#alias
     * @default "@/components/ui"
     */
    componentDir: "@/components/shadcn",
  },

  srcDir: "src/app",
  serverDir: "src/server",
  dir: {
    public: "src/public",
  },
});

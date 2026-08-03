import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  async rewrites() {
    const apiBaseUrl = process.env.API_BASE_URL;
    if (!apiBaseUrl) return [];

    return [
      {
        source: "/api/signalr-hub/:path*",
        destination: `${apiBaseUrl.replace(/\/$/, "")}/signalr-hub/:path*`,
      },
    ];
  },
};

export default nextConfig;

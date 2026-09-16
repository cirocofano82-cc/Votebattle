/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  // Produce a self-contained server build for a small Docker image.
  output: "standalone",
  // Contender/battle images can come from any host (admin-provided URLs);
  // we use plain <img> tags, so no next/image domain config is required.
};

export default nextConfig;

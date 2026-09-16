import LegalLayout, { Section } from "@/components/LegalLayout";

export const metadata = { title: "Cookie Policy — VoteBattle" };

export default function CookiesPage() {
  return (
    <LegalLayout title="Cookie Policy">
      <Section heading="Essential cookies">
        We use a secure, HttpOnly session cookie to keep you signed in. This is required for the
        service to function.
      </Section>
      <Section heading="Third parties">
        Stripe and Cloudflare Turnstile may set their own cookies during checkout and bot checks.
      </Section>
      <Section heading="Your choices">
        Placeholder — if analytics or non-essential cookies are added later, provide a consent banner
        and controls in line with local law.
      </Section>
    </LegalLayout>
  );
}

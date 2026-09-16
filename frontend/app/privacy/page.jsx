import LegalLayout, { Section } from "@/components/LegalLayout";

export const metadata = { title: "Privacy Policy — VoteBattle" };

export default function PrivacyPage() {
  return (
    <LegalLayout title="Privacy Policy">
      <Section heading="Data we collect">
        Account data (username, email), activity (votes, comments), and payment metadata from Stripe.
        We do not store card details.
      </Section>
      <Section heading="How we use it">
        To run the service, prevent abuse, process purchases and show your own history and stats.
      </Section>
      <Section heading="Processors">
        We use Stripe (payments), an email provider (transactional email) and Cloudflare Turnstile
        (bot protection). Each processes data on our behalf.
      </Section>
      <Section heading="Your rights">
        You may request access to or deletion of your personal data, subject to legal retention
        requirements for financial records.
      </Section>
      <Section heading="Contact">
        Placeholder — add a data controller and contact address, and complete this policy with legal
        review (e.g. GDPR/CCPA as applicable).
      </Section>
    </LegalLayout>
  );
}

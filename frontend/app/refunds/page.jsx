import LegalLayout, { Section } from "@/components/LegalLayout";

export const metadata = { title: "Refund Policy — VoteBattle" };

export default function RefundsPage() {
  return (
    <LegalLayout title="Refund Policy">
      <Section heading="Digital goods">
        Vote Credits are digital goods delivered instantly. Once credits have been spent on votes,
        those votes are final and cannot be reversed.
      </Section>
      <Section heading="Requesting a refund">
        If credits were purchased in error and remain unused, contact support. Approved refunds mark
        the payment as refunded and remove the corresponding credits.
      </Section>
      <Section heading="Chargebacks">
        Fraudulent chargebacks may result in account suspension. Placeholder — align this policy with
        applicable consumer-protection law and Stripe's rules before launch.
      </Section>
    </LegalLayout>
  );
}

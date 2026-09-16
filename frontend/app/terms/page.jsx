import LegalLayout, { Section } from "@/components/LegalLayout";

export const metadata = { title: "Terms of Service — VoteBattle" };

export default function TermsPage() {
  return (
    <LegalLayout title="Terms of Service">
      <Section heading="1. The service">
        VoteBattle lets you compare two contenders and support your side using Vote Credits.
        Credits are used to express and back opinions; they are not a game of chance and carry no
        cash prize.
      </Section>
      <Section heading="2. Accounts">
        You must provide a valid email and keep your credentials secure. Accounts that violate these
        terms may be suspended or banned.
      </Section>
      <Section heading="3. Vote Credits & purchases">
        Vote Credits are a digital good with no monetary value and are non-transferable. New verified
        accounts receive a one-time bonus of free credits. Purchases are processed by Stripe.
      </Section>
      <Section heading="4. Acceptable use">
        No spam, harassment, hate speech, illegal content or attempts to manipulate the platform
        beyond ordinary paid voting. See the Community Guidelines.
      </Section>
      <Section heading="5. Liability & changes">
        The service is provided "as is". We may update these terms; continued use constitutes
        acceptance. Placeholder — final wording pending legal review.
      </Section>
    </LegalLayout>
  );
}

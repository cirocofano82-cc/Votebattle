import LegalLayout, { Section } from "@/components/LegalLayout";

export const metadata = { title: "Community Guidelines — VoteBattle" };

export default function CommunityGuidelinesPage() {
  return (
    <LegalLayout title="Community Guidelines">
      <Section heading="Keep it fair">
        Back your side with credits — that's the game. Don't try to exploit bugs, create fake
        accounts, or farm the free-credit bonus.
      </Section>
      <Section heading="Be respectful">
        No harassment, hate speech, or personal attacks in comments. Debate the contenders, not the
        people.
      </Section>
      <Section heading="Keep it clean">
        No spam, misleading content, or copyright-infringing material in battles or comments.
      </Section>
      <Section heading="Enforcement">
        Violations can lead to removed content, suspension, or a ban. Report anything that breaks
        these rules. Placeholder — expand with examples and an appeals process before launch.
      </Section>
    </LegalLayout>
  );
}

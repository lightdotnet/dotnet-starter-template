// Placeholder data for the dashboard overview demo — this UI shell does not
// call src/Identity.Api yet, see the plan's "Explicitly out of scope" section.

export interface SampleUser {
  id: string;
  name: string;
  email: string;
  role: "Admin" | "Editor" | "Viewer";
  status: "Active" | "Invited" | "Suspended";
  joinedAt: string;
}

export const SAMPLE_USERS: SampleUser[] = [
  {
    id: "1",
    name: "Minh Vu",
    email: "minhvu@example.com",
    role: "Admin",
    status: "Active",
    joinedAt: "2026-01-14",
  },
  {
    id: "2",
    name: "Ava Nguyen",
    email: "ava.nguyen@example.com",
    role: "Editor",
    status: "Active",
    joinedAt: "2026-02-02",
  },
  {
    id: "3",
    name: "Liam Tran",
    email: "liam.tran@example.com",
    role: "Viewer",
    status: "Invited",
    joinedAt: "2026-02-20",
  },
  {
    id: "4",
    name: "Sophia Le",
    email: "sophia.le@example.com",
    role: "Editor",
    status: "Active",
    joinedAt: "2026-03-05",
  },
  {
    id: "5",
    name: "Noah Pham",
    email: "noah.pham@example.com",
    role: "Viewer",
    status: "Suspended",
    joinedAt: "2026-03-18",
  },
  {
    id: "6",
    name: "Mia Vo",
    email: "mia.vo@example.com",
    role: "Admin",
    status: "Active",
    joinedAt: "2026-04-01",
  },
  {
    id: "7",
    name: "Ethan Dang",
    email: "ethan.dang@example.com",
    role: "Editor",
    status: "Invited",
    joinedAt: "2026-04-16",
  },
  {
    id: "8",
    name: "Isabella Bui",
    email: "isabella.bui@example.com",
    role: "Viewer",
    status: "Active",
    joinedAt: "2026-05-02",
  },
  {
    id: "9",
    name: "Lucas Ho",
    email: "lucas.ho@example.com",
    role: "Editor",
    status: "Active",
    joinedAt: "2026-05-21",
  },
  {
    id: "10",
    name: "Amelia Dinh",
    email: "amelia.dinh@example.com",
    role: "Viewer",
    status: "Active",
    joinedAt: "2026-06-09",
  },
  {
    id: "11",
    name: "James Trinh",
    email: "james.trinh@example.com",
    role: "Admin",
    status: "Active",
    joinedAt: "2026-06-27",
  },
  {
    id: "12",
    name: "Charlotte Ly",
    email: "charlotte.ly@example.com",
    role: "Viewer",
    status: "Suspended",
    joinedAt: "2026-07-11",
  },
];

export interface StatSummary {
  label: string;
  value: string;
  delta: string;
  trend: "up" | "down";
}

export const STAT_SUMMARIES: StatSummary[] = [
  { label: "Total Users", value: "1,284", delta: "+4.6%", trend: "up" },
  { label: "Active Sessions", value: "342", delta: "+1.2%", trend: "up" },
  { label: "Pending Invites", value: "18", delta: "-2.1%", trend: "down" },
  { label: "Suspended Accounts", value: "6", delta: "-0.4%", trend: "down" },
];

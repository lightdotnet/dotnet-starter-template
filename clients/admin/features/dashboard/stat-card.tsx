import { TrendingDown, TrendingUp } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import type { StatSummary } from "@/features/dashboard/sample-data";

export function StatCard({ label, value, delta, trend }: StatSummary) {
  const TrendIcon = trend === "up" ? TrendingUp : TrendingDown;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-sm font-normal text-muted-foreground">
          {label}
        </CardTitle>
      </CardHeader>
      <CardContent className="flex items-end justify-between gap-2">
        <span className="text-2xl font-semibold tracking-tight">{value}</span>
        <span
          className={cn(
            "flex items-center gap-1 text-xs font-medium",
            trend === "up" ? "text-primary" : "text-destructive",
          )}
        >
          <TrendIcon className="size-3.5" />
          {delta}
        </span>
      </CardContent>
    </Card>
  );
}

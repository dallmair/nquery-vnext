using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class AtMostOneRowReorderer : BoundTreeRewriter
    {
        protected override BoundRelation RewriteTopRelation(BoundTopRelation node)
        {
            var input = RewriteRelation(node.Input);
            var estimate = CardinalityEstimator.Estimate(input);
            return estimate.Maximum <= node.Limit ? input : node;
        }

        protected override BoundRelation RewriteSortRelation(BoundSortRelation node)
        {
            var input = RewriteRelation(node.Input);
            return WillProduceAtMostOneRow(input) ? input : node;
        }

        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            // Reorder
            //
            // A LSJ (B LOJ C)      --->    (A LOJ C) LSJ B     (LOJ has no join condition and C produces at most one row)

            if (node.JoinType == BoundJoinType.LeftSemi ||
                node.JoinType == BoundJoinType.LeftAntiSemi)
            {
                var rightChildJoin = node.Right as BoundJoinRelation;

                if (rightChildJoin != null &&
                    rightChildJoin.JoinType == BoundJoinType.LeftOuter &&
                    rightChildJoin.Condition == null &&
                    WillProduceAtMostOneRow(rightChildJoin.Right))
                {
                    var newLeft = rightChildJoin.WithLeft(node.Left);
                    var newNode = node.Update(node.JoinType,
                                              newLeft,
                                              rightChildJoin.Left,
                                              node.Condition,
                                              node.Probe,
                                              node.PassthruPredicate);
                    return RewriteRelation(newNode);
                }
            }

            return node.Update(node.JoinType,
                               RewriteRelation(node.Left),
                               RewriteRelation(node.Right),
                               node.Condition,
                               node.Probe,
                               node.PassthruPredicate);
        }

        private static bool WillProduceAtMostOneRow(BoundRelation node)
        {
            var estimate = CardinalityEstimator.Estimate(node);
            return estimate.Maximum <= 1;
        }
    }
}
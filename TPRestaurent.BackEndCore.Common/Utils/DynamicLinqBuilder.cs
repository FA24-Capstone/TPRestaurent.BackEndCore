using System.Linq.Expressions;

public static class DynamicLinqBuilder<T>
{
    public static Expression<Func<T, bool>> BuildExpression(List<Func<Expression<Func<T, bool>>>> conditions)
    {
        if (conditions == null || !conditions.Any())
            return entity => true;

        var parameter = Expression.Parameter(typeof(T), "entity");
        Expression body = null;

        foreach (var condition in conditions)
        {
            var conditionExpression = condition().Body;
            var replacedExpression = new ParameterReplacer(parameter).Visit(conditionExpression);

            if (body == null)
            {
                body = replacedExpression;
            }
            else
            {
                body = Expression.AndAlso(body, replacedExpression);
            }
        }

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterReplacer(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }
    }
}